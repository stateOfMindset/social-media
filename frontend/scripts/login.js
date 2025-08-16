const apiUrl = "http://localhost:5000/api/users";

async function login() {
    const username = document.getElementById("loginUsername").value.trim();
    const password = document.getElementById("loginPassword").value.trim();

    if (!username || !password) {
        alert("Please fill in all fields.");
        return;
    }

    const res = await fetch(`${apiUrl}/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username: username, passwordHash: password })
    });

    if (res.ok) {
        const data = await res.json();
        // Save user info in localStorage
        localStorage.setItem("username", data.username);
        localStorage.setItem("role", data.role || "User");
        window.location.href = "index.html";
    } else {
        alert(await res.text());
    }
}

async function signup() {
    const username = document.getElementById("signupUsername").value.trim();
    const password = document.getElementById("signupPassword").value.trim();

    if (!username || !password) {
        alert("Please fill in all fields.");
        return;
    }

    const res = await fetch(`${apiUrl}/signup`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username: username, passwordHash: password })
    });

    if (res.ok) {
        alert("Signup successful! You can now log in.");
    } else {
        alert(await res.text());
    }
}

// Attach event handlers
document.getElementById("loginBtn").onclick = login;
document.getElementById("signupBtn").onclick = signup;
