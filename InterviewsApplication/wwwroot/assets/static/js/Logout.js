document.addEventListener("DOMContentLoaded", () => {
    const logoutButton = document.getElementById("logoutBtn");

    if (logoutButton) {
        logoutButton.addEventListener("click", async (e) => {
            e.preventDefault();
            console.log("Logout button clicked");

            try {
                const response = await fetch("https://localhost:7286/api/login/logout", {
                    method: "POST",
                    credentials: "include"
                });

                if (!response.ok) {
                    throw new Error("Logout failed");
                }

                sessionStorage.clear();
                localStorage.clear();

                window.location.href = "https://localhost:7286/admin-login.html";
            } catch (error) {
                console.error("Logout error:", error);
                alert("Something went wrong during logout.");
            }
        });
    }
});
