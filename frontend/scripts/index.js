const apiUrl = "http://localhost:5000/api/posts";

// Utility: safe text setter
function setText(id, value) {
  const el = document.getElementById(id);
  if (el) el.textContent = value;
}

// Check login status
const username = localStorage.getItem("username");
const role = localStorage.getItem("role") || "User";
const isAdmin = role.toLowerCase() === "admin" || localStorage.getItem("isAdmin") === "true";

if (!username) {
  window.location.href = "login.html";
} else {
  setText("userDisplay", username);
  setText("userDisplayNav", `Logged in as: ${username}`);
}

if (isAdmin) {
  const adminLink = document.getElementById("adminLink");
  if (adminLink) adminLink.style.display = "inline";
}

// Logout
const logoutBtn = document.getElementById("logoutBtn");
if (logoutBtn) {
  logoutBtn.addEventListener("click", () => {
    localStorage.clear();
    window.location.href = "login.html";
  });
}

function validatePostContent(text) {
  const trimmed = (text || "").trim();
  if (!trimmed) return { ok: false, msg: "Post content cannot be empty." };
  if (trimmed.length > 1000) return { ok: false, msg: "Post is too long (max 1000 chars)." };
  return { ok: true, value: trimmed };
}

async function loadPosts() {
  const res = await fetch(apiUrl, { method: "GET" });
  if (!res.ok) {
    alert("Failed to load posts.");
    return;
  }
  const posts = await res.json();

  const postsDiv = document.getElementById("posts");
  postsDiv.innerHTML = "";

  posts.forEach(post => {
    const card = document.createElement("div");
    card.classList.add("post");

    // Header (no innerHTML)
    const header = document.createElement("div");
    header.classList.add("post-header");

    const u = document.createElement("span");
    u.textContent = post.username;

    const t = document.createElement("span");
    t.textContent = new Date(post.createdAt).toLocaleString();

    header.appendChild(u);
    header.appendChild(t);
    card.appendChild(header);

    // Content (use textContent)
    const content = document.createElement("p");
    content.textContent = post.content;
    card.appendChild(content);

    // Actions (delete button for own posts or admin)
    if (post.username === username || isAdmin) {
      const actions = document.createElement("div");
      actions.classList.add("post-actions");

      const deleteBtn = document.createElement("button");
      deleteBtn.type = "button";
      deleteBtn.textContent = "Delete";
      deleteBtn.addEventListener("click", () => deletePost(post.id));

      actions.appendChild(deleteBtn);
      card.appendChild(actions);
    }

    postsDiv.appendChild(card);
  });
}

async function submitPost() {
  const content = document.getElementById("postText").value;
  const { ok, msg, value } = validatePostContent(content);
  if (!ok) return alert(msg);

  const res = await fetch(apiUrl, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ content: value, username })
  });

  if (res.ok) {
    document.getElementById("postText").value = "";
    loadPosts();
  } else {
    alert(await res.text());
  }
}

async function deletePost(id) {
  if (!confirm("Are you sure you want to delete this post?")) return;

  const res = await fetch(`${apiUrl}/${encodeURIComponent(id)}`, { method: "DELETE" });
  if (res.ok) {
    loadPosts();
  } else {
    alert(await res.text());
  }
}

const submitBtn = document.getElementById("submitPost");
if (submitBtn) submitBtn.addEventListener("click", submitPost);

// Initial load
loadPosts();