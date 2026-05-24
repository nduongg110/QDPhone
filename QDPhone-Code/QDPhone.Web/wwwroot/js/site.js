window.showToast = function (message, type) {
    const container = document.getElementById("toastContainer");
    if (!container) return;

    const color = type === "success" ? "text-bg-success" : type === "warning" ? "text-bg-warning" : "text-bg-danger";
    const toast = document.createElement("div");
    toast.className = `toast align-items-center ${color} border-0`;
    toast.setAttribute("role", "alert");
    toast.setAttribute("aria-live", "assertive");
    toast.setAttribute("aria-atomic", "true");
    toast.innerHTML = `<div class="d-flex"><div class="toast-body">${message}</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>`;
    container.appendChild(toast);

    const bsToast = new bootstrap.Toast(toast, { delay: 3000 });
    bsToast.show();
    toast.addEventListener("hidden.bs.toast", function () { toast.remove(); });
};

window.changePrimaryImage = function (src) {
    const img = document.getElementById("primaryProductImage");
    if (img) img.src = src;
};

function pad(num) {
    return num.toString().padStart(2, "0");
}

function startCountdown() {
    const countdownEl = document.querySelector(".flash-sale-countdown[data-target-time]");
    if (!countdownEl) return;
    const target = new Date(countdownEl.getAttribute("data-target-time")).getTime();

    const tick = function () {
        const now = new Date().getTime();
        const diff = Math.max(0, target - now);
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((diff % (1000 * 60)) / 1000);
        countdownEl.textContent = `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`;
    };

    tick();
    setInterval(tick, 1000);
}

function getQuickCategoryItemsPerSlide() {
    if (window.innerWidth >= 1200) return 6;
    if (window.innerWidth >= 768) return 4;
    return 2;
}

function renderQuickCategorySlides() {
    const carousel = document.getElementById("quickCategoryCarousel");
    const template = document.getElementById("quickCategoryTemplate");
    if (!carousel || !template) return;

    const inner = carousel.querySelector(".carousel-inner");
    if (!inner) return;

    const sourceItems = Array.from(template.querySelectorAll(".quick-category"));
    if (!sourceItems.length) return;

    const itemsPerSlide = getQuickCategoryItemsPerSlide();
    const totalSlides = Math.ceil(sourceItems.length / itemsPerSlide);

    inner.innerHTML = "";

    for (let i = 0; i < totalSlides; i++) {
        const slide = document.createElement("div");
        slide.className = `carousel-item ${i === 0 ? "active" : ""}`;

        const wrap = document.createElement("div");
        wrap.className = "quick-categories d-flex gap-2 pb-2";

        const chunk = sourceItems.slice(i * itemsPerSlide, (i + 1) * itemsPerSlide);
        chunk.forEach((item) => wrap.appendChild(item.cloneNode(true)));

        slide.appendChild(wrap);
        inner.appendChild(slide);
    }

    const controls = carousel.querySelectorAll(".carousel-control-prev, .carousel-control-next");
    controls.forEach((control) => {
        control.style.display = totalSlides > 1 ? "" : "none";
    });

    if (window.bootstrap && bootstrap.Carousel) {
        bootstrap.Carousel.getOrCreateInstance(carousel);
    }
}

function initQuickCategoryResponsiveCarousel() {
    let currentBucket = getQuickCategoryItemsPerSlide();
    renderQuickCategorySlides();

    window.addEventListener("resize", () => {
        const nextBucket = getQuickCategoryItemsPerSlide();
        if (nextBucket !== currentBucket) {
            currentBucket = nextBucket;
            renderQuickCategorySlides();
        }
    });
}

function initFloatingChat() {
    const btnChatNow = document.getElementById("btnChatNow");
    const btnChatClose = document.getElementById("btnChatClose");
    const chatPanel = document.getElementById("chatPanel");
    if (!btnChatNow || !chatPanel) return;

    btnChatNow.addEventListener("click", function () {
        chatPanel.classList.toggle("d-none");
    });

    if (btnChatClose) {
        btnChatClose.addEventListener("click", function () {
            chatPanel.classList.add("d-none");
        });
    }
}

document.addEventListener("DOMContentLoaded", function () {
    startCountdown();
    initQuickCategoryResponsiveCarousel();
    initFloatingChat();
});
