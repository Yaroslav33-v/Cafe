const adminTeleportation = document.getElementById('admin-relocation');
adminTeleportation.addEventListener('click', async () => {
    try {
        const response = await fetch(`/me`);
        const data = await response.json();

        if (data.role === "admin") {
            document.location.replace("/admin/index");
        }
    }
    catch {

    }
});