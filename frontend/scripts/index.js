const apiUrl = "http://localhost:5000/api/posts";

// Check login status
const username = localStorage.getItem("username");
if (!username) {
    window.location.href = "login.html";
} else {
    document.getElementById("userDisplay").textContent = username;
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
    postsDiv.innerHTML = ""; // clear existing posts

    posts.forEach(post => {
        const div = document.createElement("div");
        div.setAttribute("data-username", post.username);
        const dateStr = new Date(post.createdAt).toLocaleString();
        div.setAttribute("data-date", dateStr);
        div.innerHTML = `<p>${post.content}</p>`;
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

document.getElementById("submitPost").onclick = submitPost;

// Initial load
loadPosts();
