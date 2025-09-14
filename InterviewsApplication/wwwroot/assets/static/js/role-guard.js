window.addEventListener('DOMContentLoaded', async () => {
    try {
        const response = await fetch('https://localhost:7286/api/UserInfo/me', {
            credentials: 'include'
        });

        if (!response.ok) {
            console.warn("User not authenticated, hiding all role-based content.");
            return;
        }

        const user = await response.json();
        const role = user.role;
        const department = user.department;

        const restrictedIds = ['menu-agents', 'menu-mail', 'menu-schedule'];
        restrictedIds.forEach(id => {
            const el = document.getElementById(id);
            if (el) el.style.display = 'none';
        });
        if (department === 'Medical' || department === 'English') {
            const reviewMenu = document.getElementById('menu-review');
            if (reviewMenu) reviewMenu.style.display = 'none';
        }

        if (role === 'SuperAdmin') {
            showMenu(['menu-agents', 'menu-mail', 'menu-schedule', 'menu-password', 'menu-stages', 'menu-interviews', 'menu-booking']);
        } else if (role === 'Admin') {
            showMenu(['menu-agents', 'menu-password', 'menu-stages', 'menu-interviews', 'menu-booking']);
        } else if (role === 'Agent') {
            showMenu(['menu-password', 'menu-stages', 'menu-interviews', 'menu-booking']);
        }

    } catch (error) {
        console.error("Error checking user role:", error);
    }
});

function showMenu(ids) {
    ids.forEach(id => {
        const el = document.getElementById(id);
        if (el) el.style.display = 'block';
    });
}