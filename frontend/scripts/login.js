const apiUrl = "http://localhost:5000/api/users";

function validUsername(u) {
  const username = (u || "").trim();
  if (!username) return { ok: false, msg: "Username is required." };
  // Letters, numbers, underscore, dot, hyphen; 3–32 chars.
  if (!/^[A-Za-z0-9_.-]{3,32}$/.test(username)) {
    return { ok: false, msg: "Username must be 3-32 chars: letters, numbers, _ . -" };
  }
  return { ok: true, value: username };
}

function validPassword(p) {
  const password = (p || "").trim();
  if (!password) return { ok: false, msg: "Password is required." };
  if (password.length < 8) return { ok: false, msg: "Password must be at least 8 characters." };
  return { ok: true, value: password };
}

async function login() {
  const u = validUsername(document.getElementById("loginUsername").value);
  if (!u.ok) return alert(u.msg);
  const p = validPassword(document.getElementById("loginPassword").value);
  if (!p.ok) return alert(p.msg);

  const res = await fetch(`${apiUrl}/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ Username: u.value, PasswordHash: p.value })
  });

  if (res.ok) {
    const data = await res.json();
    // Save minimal client state; real auth should use HttpOnly cookies server-side.
    localStorage.setItem("username", data.username);
    if (data.role) {
      localStorage.setItem("role", data.role);
      localStorage.setItem("isAdmin", String(data.role.toLowerCase() === "admin"));
    }
    window.location.href = "index.html";
  } else {
    alert(await res.text());
  }
}

async function signup() {
  const u = validUsername(document.getElementById("signupUsername").value);
  if (!u.ok) return alert(u.msg);
  const p = validPassword(document.getElementById("signupPassword").value);
  if (!p.ok) return alert(p.msg);

  const res = await fetch(`${apiUrl}/signup`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ Username: u.value, PasswordHash: p.value })
  });

  if (res.ok) {
    alert("Signup successful! You can now log in.");
  } else {
    alert(await res.text());
  }
}

// Attach event handlers (no inline handlers — CSP friendly)
const loginBtn = document.getElementById("loginBtn");
if (loginBtn) loginBtn.addEventListener("click", login);
const signupBtn = document.getElementById("signupBtn");
if (signupBtn) signupBtn.addEventListener("click", signup);