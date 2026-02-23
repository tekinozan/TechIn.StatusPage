(function () {
    var INTERVAL = window.__refreshInterval || 60;
    var circumference = 2 * Math.PI * 6; // r=6 in the SVG ring
    var elapsed = 0;

    var el = document.getElementById('refresh-indicator');
    var textEl = document.getElementById('refresh-text');
    var ringEl = document.getElementById('refresh-ring-fg');

    if (!el || !textEl) return;

    function formatAgo(sec) {
        if (sec < 5) return 'just now';
        if (sec < 60) return sec + 's ago';
        var m = Math.floor(sec / 60);
        return m + 'm ' + (sec % 60) + 's ago';
    }

    function tick() {
        elapsed++;
        textEl.textContent = 'Updated ' + formatAgo(elapsed);

        // Update ring progress
        if (ringEl) {
            var progress = Math.min(elapsed / INTERVAL, 1);
            ringEl.style.strokeDashoffset = circumference * (1 - progress);
        }

        // Time to refresh
        if (elapsed >= INTERVAL) {
            el.classList.add('refreshing');
            textEl.textContent = 'Refreshing\u2026';
            setTimeout(function () { location.reload(); }, 300);
            return;
        }

        setTimeout(tick, 1000);
    }

    // Start the timer
    setTimeout(tick, 1000);
})();