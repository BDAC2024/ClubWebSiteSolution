export function initialise(container) {
    if (!container) {
        throw new Error(
            "MobileTabScroller container was not supplied.");
    }

    let scrollElement = null;
    let resizeObserver = null;
    let mutationObserver = null;
    let disposed = false;

    const tolerance = 2;

    function findScrollElement() {
        /*
         * Syncfusion's internal markup can differ between versions.
         * Check the likely horizontal scrolling elements first.
         */
        const selectors = [
            ".e-tab-header .e-hscroll-content",
            ".e-tab-header .e-toolbar-items",
            ".e-tab-header .e-hscroll-bar",
            ".e-tab-header"
        ];

        for (const selector of selectors) {
            const candidate = container.querySelector(selector);

            if (isHorizontallyScrollable(candidate)) {
                return candidate;
            }
        }

        /*
         * Final fallback: inspect descendants of the tab header and
         * find the element whose scroll width exceeds its client width.
         */
        const header = container.querySelector(".e-tab-header");

        if (!header) {
            return null;
        }

        const descendants = header.querySelectorAll("*");

        for (const candidate of descendants) {
            if (isHorizontallyScrollable(candidate)) {
                return candidate;
            }
        }

        return null;
    }

    function isHorizontallyScrollable(element) {
        if (!element) {
            return false;
        }

        return element.scrollWidth >
            element.clientWidth + tolerance;
    }

    function updateArrowState() {
        if (disposed) {
            return;
        }

        if (!scrollElement ||
            !scrollElement.isConnected) {
            attachToScrollElement();
        }

        if (!scrollElement) {
            container.classList.remove(
                "can-scroll-left",
                "can-scroll-right");

            return;
        }

        const maximumScrollLeft =
            scrollElement.scrollWidth -
            scrollElement.clientWidth;

        const currentScrollLeft =
            normaliseScrollLeft(scrollElement);

        const canScrollLeft =
            currentScrollLeft > tolerance;

        const canScrollRight =
            currentScrollLeft <
            maximumScrollLeft - tolerance;

        container.classList.toggle(
            "can-scroll-left",
            canScrollLeft);

        container.classList.toggle(
            "can-scroll-right",
            canScrollRight);
    }

    function normaliseScrollLeft(element) {
        /*
         * This component is expected to be used in a left-to-right
         * layout. Math.abs also avoids small negative bounce values
         * reported by some browsers.
         */
        return Math.abs(element.scrollLeft);
    }

    function attachToScrollElement() {
        if (scrollElement) {
            scrollElement.removeEventListener(
                "scroll",
                updateArrowState);
        }

        scrollElement = findScrollElement();

        if (!scrollElement) {
            return;
        }

        scrollElement.addEventListener(
            "scroll",
            updateArrowState,
            {
                passive: true
            });
    }

    function scheduleUpdate() {
        /*
         * Syncfusion may finish rendering its internal tab header
         * after the Blazor wrapper itself has rendered.
         */
        window.requestAnimationFrame(() => {
            attachToScrollElement();
            updateArrowState();
        });
    }

    function scroll(amount) {
        if (disposed) {
            return;
        }

        if (!scrollElement ||
            !scrollElement.isConnected) {
            attachToScrollElement();
        }

        if (!scrollElement) {
            return;
        }

        scrollElement.scrollBy({
            left: amount,
            behavior: "smooth"
        });

        /*
         * The scroll event normally updates the state. These delayed
         * checks also cover browsers that coalesce smooth-scroll events.
         */
        window.setTimeout(updateArrowState, 100);
        window.setTimeout(updateArrowState, 350);
    }

    function dispose() {
        disposed = true;

        if (scrollElement) {
            scrollElement.removeEventListener(
                "scroll",
                updateArrowState);
        }

        resizeObserver?.disconnect();
        mutationObserver?.disconnect();

        container.classList.remove(
            "can-scroll-left",
            "can-scroll-right");

        scrollElement = null;
    }

    resizeObserver = new ResizeObserver(() => {
        scheduleUpdate();
    });

    resizeObserver.observe(container);

    mutationObserver = new MutationObserver(() => {
        scheduleUpdate();
    });

    mutationObserver.observe(container, {
        childList: true,
        subtree: true
    });

    /*
     * Perform several initial checks because the Syncfusion component
     * may render or measure its header over more than one frame.
     */
    scheduleUpdate();
    window.setTimeout(scheduleUpdate, 100);
    window.setTimeout(scheduleUpdate, 300);

    return {
        scroll,
        dispose
    };
}