// 🔒 Safe helper to escape/strip potential dangerous input
function sanitizeInput(str) {
  const temp = document.createElement("div");
  temp.textContent = str;  // textContent ensures no HTML is executed
  return temp.innerHTML;
}

async function fetchPosts() {
  try {
    const response = await fetch("/api/posts", {
      method: "GET",
      credentials: "include", // secure cookie sessions
      headers: { "Accept": "application/json" }
    });

    if (!response.ok) throw new Error("Failed to fetch posts");

    const posts = await response.json();
    renderPosts(posts);
  } catch (err) {
    console.error("Error fetching posts:", err);
    document.getElementById("postsList").textContent = "Could not load posts.";
  }
}

function renderPosts(posts) {
  const postsList = document.getElementById("postsList");
  postsList.innerHTML = "";

  if (!Array.isArray(posts) || posts.length === 0) {
    postsList.textContent = "No posts yet.";
    return;
  }

  posts.forEach(post => {
    const postDiv = document.createElement("div");
    postDiv.className = "post";

    const userSpan = document.createElement("span");
    userSpan.className = "post-user";
    userSpan.textContent = sanitizeInput(post.username || "Unknown");

    const textP = document.createElement("p");
    textP.className = "post-text";
    textP.textContent = sanitizeInput(post.text || "");

    postDiv.appendChild(userSpan);
    postDiv.appendChild(textP);
    postsList.appendChild(postDiv);
  });
}

async function submitPost() {
  const newPost = document.getElementById("newPost").value.trim();

  // 🔒 Validate before sending
  if (newPost.length === 0) {
    alert("Post cannot be empty.");
    return;
  }
  if (newPost.length > 500) {
    alert("Post too long (max 500 characters).");
    return;
  }

  try {
    const response = await fetch("/api/posts", {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json"
      },
      body: JSON.stringify({ text: newPost })
    });

    if (!response.ok) {
      const errData = await response.json().catch(() => ({}));
      throw new Error(errData.message || "Failed to submit post");
    }

    document.getElementById("newPost").value = "";
    await fetchPosts(); // Refresh posts
  } catch (err) {
    console.error("Error submitting post:", err);
    alert("Failed to submit post. Please try again.");
  }
}

document.getElementById("submitPost").addEventListener("click", submitPost);

// Load posts on page ready
window.addEventListener("DOMContentLoaded", fetchPosts);
