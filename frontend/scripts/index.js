const apiUrl = "http://localhost:5000/api/posts";

// Check login status
const username = localStorage.getItem("username");
const isAdmin = localStorage.getItem("isAdmin") === "true";

if (!username) {
    window.location.href = "login.html";
} else {
    document.getElementById("userDisplay").textContent = username;
    document.getElementById("userDisplayNav").textContent = `Logged in as: ${username}`;
}

if (isAdmin) {
    document.getElementById("adminLink").style.display = "inline";
}

document.getElementById("logoutBtn").onclick = () => {
    localStorage.clear();
    window.location.href = "login.html";
};

async function loadPosts() {
    const res = await fetch(apiUrl);
    if (!res.ok) {
        alert("Failed to load posts.");
        return;
    }
    const posts = await res.json();

    const postsDiv = document.getElementById("posts");
    postsDiv.innerHTML = "";

    posts.forEach(post => {
        const div = document.createElement("div");
        div.classList.add("post");

        // Header
        const header = document.createElement("div");
        header.classList.add("post-header");
        header.innerHTML = `<span>${post.username}</span> <span>${new Date(post.createdAt).toLocaleString()}</span>`;
        div.appendChild(header);

        // Content
        const content = document.createElement("p");
        content.textContent = post.content;
        div.appendChild(content);

        // Actions (delete button for own posts or admin)
        if (post.username === username || isAdmin) {
            const actions = document.createElement("div");
            actions.classList.add("post-actions");
            const deleteBtn = document.createElement("button");
            deleteBtn.textContent = "Delete";
            deleteBtn.onclick = () => deletePost(post.id);
            actions.appendChild(deleteBtn);
            div.appendChild(actions);
        }

        postsDiv.appendChild(div);
    });
}

async function submitPost() {
    const content = document.getElementById("postText").value.trim();
    if (!content) {
        alert("Post content cannot be empty.");
        return;
    }

    const res = await fetch(apiUrl, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ content, username })
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

    const res = await fetch(`${apiUrl}/${id}`, {
        method: "DELETE"
    });

    if (res.ok) {
        loadPosts();
    } else {
        alert(await res.text());
    }
}

document.getElementById("submitPost").onclick = submitPost;

// Initial load
loadPosts();
