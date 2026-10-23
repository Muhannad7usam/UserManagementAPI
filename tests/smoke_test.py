#!/usr/bin/env python3
"""Dependency-free end-to-end tests for the User Management API.

Start the API with `dotnet run --project UserManagementAPI` and then run:
  python3 tests/smoke_test.py --base-url http://localhost:5100
"""

import argparse
import json
import sys
import time
import urllib.error
import urllib.request
import uuid


def request(base, method, path, payload=None, api_key=None):
    data = None if payload is None else json.dumps(payload).encode("utf-8")
    headers = {"Accept": "application/json"}
    if payload is not None:
        headers["Content-Type"] = "application/json"
    if api_key:
        headers["X-API-Key"] = api_key
    req = urllib.request.Request(base + path, data=data, headers=headers, method=method)
    try:
        resp = urllib.request.urlopen(req, timeout=5)
    except urllib.error.HTTPError as exc:
        resp = exc
    with resp:
        body = resp.read().decode("utf-8")
        return resp.status, json.loads(body) if body.strip() else None, dict(resp.headers)


def assert_status(actual, expected, message):
    if actual != expected:
        raise AssertionError(f"{message}: expected HTTP {expected}, received {actual}")
    print(f"PASS: {message} (HTTP {actual})")


def run(base, api_key):
    # Avoid CI timing flakes while the server starts.
    for _ in range(45):
        try:
            status, body, _ = request(base, "GET", "/health")
            if status == 200:
                break
        except (OSError, TimeoutError):
            pass
        time.sleep(0.5)
    else:
        raise RuntimeError("API did not become ready; run `dotnet run` first.")
    assert_status(status, 200, "health endpoint")

    unique = uuid.uuid4().hex[:12]
    email = f"test-{unique}@example.com"
    second_email = f"test-other-{unique}@example.com"
    user = None
    second = None

    try:
        status, body, _ = request(base, "GET", "/api/users", api_key=api_key)
        assert_status(status, 200, "list users")
        assert isinstance(body, list), "GET all must return an array"

        status, body, headers = request(base, "POST", "/api/users", {
            "name": "  Sarah Ahmed  ", "email": email, "department": " HR "
        }, api_key)
        assert_status(status, 201, "create valid user")
        user = body["id"]
        assert body["name"] == "Sarah Ahmed" and body["department"] == "HR"
        assert headers.get("Location") == f"/api/users/{user}", "Created resource must have Location header"

        status, body, _ = request(base, "GET", "/api/users/" + user, api_key=api_key)
        assert_status(status, 200, "get user by ID")
        assert body["email"] == email

        status, body, _ = request(base, "POST", "/api/users", {
            "name": "Bad", "email": "broken-email", "department": "IT"
        }, api_key)
        assert_status(status, 400, "invalid email rejected")
        assert "errors" in body and "email" in body["errors"]

        status, body, _ = request(base, "POST", "/api/users", {
            "name": "   ", "email": f"bad-{unique}@example.com", "department": "IT"
        }, api_key)
        assert_status(status, 400, "whitespace-only name rejected")

        status, body, _ = request(base, "POST", "/api/users", {
            "name": "Missing Department", "email": f"missing-{unique}@example.com"
        }, api_key)
        assert_status(status, 400, "missing required department rejected")

        status, body, _ = request(base, "POST", "/api/users", {
            "name": "X" * 101, "email": f"long-{unique}@example.com", "department": "IT"
        }, api_key)
        assert_status(status, 400, "overlong name rejected")
        assert "name" in body["errors"]

        status, _, _ = request(base, "POST", "/api/users", {
            "name": "Duplicate", "email": email.upper(), "department": "Finance"
        }, api_key)
        assert_status(status, 409, "case-insensitive duplicate email rejected")

        status, body, _ = request(base, "POST", "/api/users", {
            "name": "Other User", "email": second_email, "department": "IT"
        }, api_key)
        assert_status(status, 201, "create second user")
        second = body["id"]

        status, _, _ = request(base, "PUT", "/api/users/" + user, {
            "name": "Sarah Updated", "email": second_email, "department": "Finance"
        }, api_key)
        assert_status(status, 409, "update with duplicate email rejected")

        status, body, _ = request(base, "PUT", "/api/users/" + user, {
            "name": "", "email": email, "department": "Finance"
        }, api_key)
        assert_status(status, 400, "update rejects missing name")

        status, body, _ = request(base, "PUT", "/api/users/" + user, {
            "name": "Sarah Updated", "email": email, "department": "IT"
        }, api_key)
        assert_status(status, 200, "update user")
        assert body["name"] == "Sarah Updated" and body["department"] == "IT"
        status, body, _ = request(base, "GET", "/api/users", api_key=api_key)
        assert_status(status, 200, "list after creation and update")
        assert any(entry["id"] == user and entry["name"] == "Sarah Updated" for entry in body)

        missing = str(uuid.uuid4())
        status, _, _ = request(base, "GET", "/api/users/" + missing, api_key=api_key)
        assert_status(status, 404, "get non-existent user")

        status, _, _ = request(base, "PUT", "/api/users/" + missing, {
            "name": "Ghost", "email": f"ghost-{unique}@example.com", "department": "IT"
        }, api_key)
        assert_status(status, 404, "update non-existent user")

        status, _, _ = request(base, "DELETE", "/api/users/" + missing, api_key=api_key)
        assert_status(status, 404, "delete non-existent user")

        if api_key:
            status, _, _ = request(base, "GET", "/api/users")
            assert_status(status, 401, "API key required when configured")
            status, _, _ = request(base, "GET", "/api/users", api_key="incorrect-secret")
            assert_status(status, 401, "incorrect API key rejected")

        deleted_user = user
        status, _, _ = request(base, "DELETE", "/api/users/" + user, api_key=api_key)
        assert_status(status, 204, "delete user")
        user = None
        status, _, _ = request(base, "GET", "/api/users/" + deleted_user, api_key=api_key)
        assert_status(status, 404, "deleted first user no longer exists")
        status, _, _ = request(base, "DELETE", "/api/users/" + second, api_key=api_key)
        assert_status(status, 204, "delete second user")
        deleted_second = second
        second = None
        status, _, _ = request(base, "GET", "/api/users/" + deleted_second, api_key=api_key)
        assert_status(status, 404, "deleted user no longer exists")
        status, body, _ = request(base, "POST", "/api/users", {
            "name": "Reused Email", "email": second_email, "department": "IT"
        }, api_key)
        assert_status(status, 201, "deleted email is available for reuse")
        second = body["id"]
        status, _, _ = request(base, "DELETE", "/api/users/" + second, api_key=api_key)
        assert_status(status, 204, "cleanup reused email user")
        second = None
        print("\nAll API smoke tests passed.")
    finally:
        if user:
            request(base, "DELETE", "/api/users/" + user, api_key=api_key)
        if second:
            request(base, "DELETE", "/api/users/" + second, api_key=api_key)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--base-url", default="http://localhost:5100")
    parser.add_argument("--api-key", default=None)
    args = parser.parse_args()
    try:
        run(args.base_url.rstrip("/"), args.api_key)
    except Exception as exc:
        print(f"FAIL: {exc}", file=sys.stderr)
        raise SystemExit(1)
