(function () {
  "use strict";

  const body = document.body;
  const sidebar = document.getElementById("adminSidebar");
  const overlay = document.querySelector("[data-sidebar-overlay]");
  const toggles = document.querySelectorAll("[data-sidebar-toggle]");
  let canUseLocalStorage = true;
  try {
    localStorage.getItem("admin-sidebar-collapsed");
  } catch (e) {
    canUseLocalStorage = false;
  }

  function toggleSidebar() {
    if (window.innerWidth < 768) {
      sidebar?.classList.toggle("mobile-open");
      overlay?.classList.toggle("show");
      return;
    }
    body.classList.toggle("admin-sidebar-collapsed");
    if (canUseLocalStorage) {
      localStorage.setItem("admin-sidebar-collapsed", body.classList.contains("admin-sidebar-collapsed") ? "1" : "0");
    }
  }

  if (canUseLocalStorage && localStorage.getItem("admin-sidebar-collapsed") === "1" && window.innerWidth >= 768) {
    body.classList.add("admin-sidebar-collapsed");
  }

  toggles.forEach((btn) => btn.addEventListener("click", toggleSidebar));
  overlay?.addEventListener("click", toggleSidebar);

  window.showToast = function (message, type) {
    const toastContainer = document.querySelector(".admin-toast-container");
    if (!toastContainer) return;
    const typeClass = type === "error" ? "danger" : type || "success";
    const el = document.createElement("div");
    el.className = `toast align-items-center text-bg-${typeClass} border-0`;
    el.setAttribute("role", "alert");
    el.setAttribute("aria-live", "assertive");
    el.setAttribute("aria-atomic", "true");
    el.innerHTML = `<div class="d-flex"><div class="toast-body">${message}</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>`;
    toastContainer.appendChild(el);
    const toast = new bootstrap.Toast(el, { delay: 3000 });
    toast.show();
    el.addEventListener("hidden.bs.toast", () => el.remove());
  };

  window.ajaxPost = function (url, data, successCallback) {
    $.post(url, data)
      .done(function (response) {
        if (typeof successCallback === "function") successCallback(response);
      })
      .fail(function () {
        window.showToast("Có lỗi xảy ra, vui lòng thử lại.", "error");
      });
  };

  window.ajaxToggle = function (url, switchEl) {
    $.post(url)
      .fail(function () {
        if (switchEl) switchEl.checked = !switchEl.checked;
        window.showToast("Không thể cập nhật trạng thái.", "error");
      });
  };

  function getOrderStatusMeta(status) {
    const map = {
      Pending: { text: "Chờ xử lý", cls: "status-pending" },
      PendingPayment: { text: "Chờ thanh toán", cls: "status-pending" },
      Paid: { text: "Đã thanh toán", cls: "status-processing" },
      Shipping: { text: "Đang giao", cls: "status-shipping" },
      Done: { text: "Hoàn tất", cls: "status-completed" },
      Cancelled: { text: "Đã hủy", cls: "status-cancelled" },
      PaymentFailed: { text: "Thanh toán thất bại", cls: "status-cancelled" }
    };
    return map[status] || { text: status, cls: "status-cancelled" };
  }

  function applyInplaceUpdate(form) {
    const updateType = form.getAttribute("data-update");
    const row = form.closest("tr");
    if (!updateType || !row) return;

    if (updateType === "order-status") {
      const selected = form.querySelector('select[name="status"]')?.value;
      if (!selected) return;
      const badge = row.querySelector(".js-order-status-badge");
      if (!badge) return;
      const meta = getOrderStatusMeta(selected);
      badge.classList.remove("status-pending", "status-processing", "status-shipping", "status-completed", "status-cancelled");
      badge.classList.add(meta.cls);
      badge.textContent = meta.text;
    }

    if (updateType === "user-role") {
      const selected = form.querySelector('select[name="role"]')?.value;
      const tag = row.querySelector(".js-user-role-display");
      if (selected && tag) tag.textContent = selected;
    }

    if (updateType === "user-active-toggle") {
      const badge = row.querySelector(".js-user-active-badge");
      const button = form.querySelector("button");
      if (!badge || !button) return;
      const isActive = badge.textContent?.trim() === "Đang hoạt động";
      if (isActive) {
        badge.textContent = "Đã khóa";
        badge.classList.remove("text-bg-success");
        badge.classList.add("text-bg-secondary");
        button.textContent = "Mở khóa";
        button.classList.remove("btn-outline-danger");
        button.classList.add("btn-outline-success");
      } else {
        badge.textContent = "Đang hoạt động";
        badge.classList.remove("text-bg-secondary");
        badge.classList.add("text-bg-success");
        button.textContent = "Khóa";
        button.classList.remove("btn-outline-success");
        button.classList.add("btn-outline-danger");
      }
    }

    if (updateType === "review-approve") {
      const badge = row.querySelector(".js-review-status-badge");
      if (badge) {
        badge.textContent = "Đã duyệt";
        badge.classList.remove("text-bg-warning");
        badge.classList.add("text-bg-success");
      }
      const approveForm = row.querySelector(".js-review-approve-form");
      approveForm?.remove();
    }
  }

  function handleAjaxForms() {
    const forms = document.querySelectorAll("form.js-ajax-form");
    forms.forEach((form) => {
      form.addEventListener("submit", function (event) {
        event.preventDefault();
        const confirmMessage = form.getAttribute("data-confirm");
        if (confirmMessage && !window.confirm(confirmMessage)) return;

        const successMessage = form.getAttribute("data-success-message") || "Cập nhật thành công.";
        const reloadMode = form.getAttribute("data-reload") || "page";
        const formData = $(form).serialize();

        $.post(form.action, formData)
          .done(function () {
            window.showToast(successMessage, "success");
            applyInplaceUpdate(form);
            if (reloadMode === "page") {
              setTimeout(() => window.location.reload(), 350);
            } else if (reloadMode === "row") {
              const row = form.closest("tr");
              if (row) row.remove();
            } else if (reloadMode === "none") {
              // intentionally no reload
            }
          })
          .fail(function () {
            window.showToast("Có lỗi xảy ra, vui lòng thử lại.", "error");
          });
      });
    });
  }

  const deleteModalEl = document.getElementById("deleteModal");
  const deleteNameEl = document.getElementById("deleteItemName");
  const deleteFormEl = document.getElementById("deleteModalForm");
  if (deleteModalEl) {
    deleteModalEl.addEventListener("show.bs.modal", function (event) {
      const btn = event.relatedTarget;
      if (!btn) return;
      const url = btn.getAttribute("data-delete-url") || "";
      const name = btn.getAttribute("data-delete-name") || "mục này";
      if (deleteNameEl) deleteNameEl.textContent = name;
      if (deleteFormEl) deleteFormEl.setAttribute("action", url);
    });
  }

  window.slugifyVi = function (value) {
    return (value || "")
      .toLowerCase()
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "")
      .replace(/đ/g, "d")
      .replace(/[^a-z0-9\s-]/g, "")
      .trim()
      .replace(/\s+/g, "-")
      .replace(/-+/g, "-");
  };

  handleAjaxForms();
})();
