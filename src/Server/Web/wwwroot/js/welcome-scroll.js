(function () {
    var ticking = false;

    function update() {
        ticking = false;
        var sections = document.querySelectorAll('[data-welcome-scroll]');
        for (var i = 0; i < sections.length; i++) {
            var rect = sections[i].getBoundingClientRect();
            var travel = rect.height - window.innerHeight;
            var p = travel > 0 ? Math.min(Math.max(-rect.top / travel, 0), 1) : 1;
            sections[i].style.setProperty('--welcome-p', p.toFixed(4));
        }
    }

    function requestUpdate() {
        if (ticking) return;
        ticking = true;
        requestAnimationFrame(update);
    }

    // The intro is meant to be scrolled from the top, so don't let the browser
    // restore an old position on refresh or back/forward.
    function resetToTop() {
        if (!document.querySelector('[data-welcome-scroll]')) return;
        if ('scrollRestoration' in history) history.scrollRestoration = 'manual';
        window.scrollTo(0, 0);
        requestUpdate();
    }

    document.addEventListener('DOMContentLoaded', resetToTop);
    window.addEventListener('pageshow', resetToTop);
    window.addEventListener('scroll', requestUpdate, { passive: true });
    window.addEventListener('resize', requestUpdate);
    document.addEventListener('DOMContentLoaded', requestUpdate);
    // Blazor enhanced navigation swaps page content without a full load.
    document.addEventListener('enhancedload', requestUpdate);
    resetToTop();
    requestUpdate();
})();
