const apiUrl = "http://localhost:5000/api/posts";

// Check login
const username = localStorage.getItem("username");
if (!username) {
    window.location.href = "login.html";
} else {
    document.getElementById("userDisplayNav").textContent = username;
}

// Logout
document.getElementById("logoutBtn").onclick = () => {
    localStorage.clear();
    window.location.href = "login.html";
};

// Load my posts
async function loadMyPosts() {
    const res = await fetch(`${apiUrl}/mine?username=${encodeURIComponent(username)}`);
    if (!res.ok) {
        alert("Failed to load your posts.");
        return;
    }
    const posts = await res.json();

    const myPostsDiv = document.getElementById("myPosts");
    myPostsDiv.innerHTML = "";

    if (posts.length === 0) {
        myPostsDiv.textContent = "You haven't posted anything yet.";
        return;
    }

    posts.forEach(post => {
        const wrapper = document.createElement("div");
        wrapper.classList.add("post");

        const date = new Date(post.createdAt).toLocaleString();
        wrapper.innerHTML = `
            <p>${post.content}</p>
            <small>Posted on ${date}</small><br>
            <button onclick="editPost(${post.id}, \`${post.content.replace(/`/g, "\\`")}\`)">Edit</button>
            <button onclick="deletePost(${post.id})">Delete</button>
        `;

        myPostsDiv.appendChild(wrapper);
    });
}

// Delete a post
async function deletePost(id) {
    if (!confirm("Are you sure you want to delete this post?")) return;

    const res = await fetch(`${apiUrl}/${id}`, { method: "DELETE" });
    if (res.ok) {
        loadMyPosts();
    } else {
        alert("Failed to delete post.");
    }
}

// Edit a post
async function editPost(id, oldContent) {
    const newContent = prompt("Edit your post:", oldContent);
    if (newContent === null || newContent.trim() === "") return;

    const res = await fetch(`${apiUrl}/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newContent.trim())
    });

    if (res.ok) {
        loadMyPosts();
    } else {
        alert("Failed to update post.");
    }
}

// Initial load
loadMyPosts();
