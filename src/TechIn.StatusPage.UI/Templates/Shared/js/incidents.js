function toggleIncidents(bar, serviceIdx, dayIdx) {
    var panel = document.getElementById('incidents-' + serviceIdx);
    var dataEl = document.getElementById('incident-data-' + serviceIdx);
    if (!panel || !dataEl) return;

    if (panel.style.display !== 'none' && panel.dataset.activeDay === String(dayIdx)) {
        closePanel(panel);
        return;
    }

    var prev = panel.parentElement.querySelector('.active-bar');
    if (prev) prev.classList.remove('active-bar');
    bar.classList.add('active-bar');

    var allDays = JSON.parse(dataEl.textContent);
    var incidents = allDays[dayIdx] || [];

    if (incidents.length === 0) {
        closePanel(panel);
        return;
    }

    panel.innerHTML = renderIncidents(incidents);
    panel.style.display = 'block';
    panel.dataset.activeDay = String(dayIdx);
}

function closePanel(panel) {
    panel.style.display = 'none';
    panel.dataset.activeDay = '';
    var active = panel.parentElement.querySelector('.active-bar');
    if (active) active.classList.remove('active-bar');
}

function renderIncidents(incidents) {
    var html = '<div class="incident-panel-header">' +
        '<span class="incident-panel-title">Events for this day</span>' +
        '<button class="incident-panel-close" onclick="closePanel(this.closest(\'.incident-panel\'))" aria-label="Close">\u2715</button>' +
        '</div>';

    for (var i = 0; i < incidents.length; i++) {
        var inc = incidents[i];
        var desc = inc.description
            ? '<div class="incident-desc">' + escHtml(inc.description) + '</div>'
            : '';
        html += '<div class="incident-item">' +
            '<div class="incident-dot ' + inc.status + '"></div>' +
            '<div class="incident-content">' +
            '<div class="incident-meta">' +
            '<span class="incident-label ' + inc.status + '">' + inc.label + '</span>' +
            '<span class="incident-time">' + inc.time + '</span>' +
            '</div>' +
            desc +
            '</div>' +
            '</div>';
    }
    return html;
}

function escHtml(s) {
    var d = document.createElement('div');
    d.textContent = s;
    return d.innerHTML;
}
