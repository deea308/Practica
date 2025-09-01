// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Auto-dismiss any alert rendered by _Flash after 4 seconds
document.addEventListener("DOMContentLoaded", () => {
    const alerts = document.querySelectorAll('.alert[data-auto-dismiss="true"]');
    alerts.forEach(el => {
        setTimeout(() => {
            try {
                const bsAlert = bootstrap.Alert.getOrCreateInstance(el);
                bsAlert.close();
            } catch { /* ignore */ }
        }, 4000);
    });
});

