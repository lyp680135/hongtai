﻿/*
 * Inline Form Validation Engine 2.6.2, jQuery plugin
 *
 * Copyright(c) 2010, Cedric Dugas
 * http://www.position-absolute.com
 *
 * 2.0 Rewrite by Olivier Refalo
 * http://www.crionics.com
 *
 * Form validation engine allowing custom regex rules to be added.
 * Licensed under the MIT License
 */
!function (a) { "use strict"; Array.indexOf || (Array.prototype.indexOf = function (a) { for (var b = 0; b < this.length; b++) if (this[b] == a) return b; return -1 }); var b = { init: function (c) { var d = this; return d.data("jqv") && null != d.data("jqv") || (c = b._saveOptions(d, c), a(document).on("click", ".formError", function () { a(this).fadeOut(150, function () { a(this).parent(".formErrorOuter").remove(), a(this).remove() }) })), this }, attach: function (c) { var e, d = this; return e = c ? b._saveOptions(d, c) : d.data("jqv"), e.validateAttribute = d.find("[data-validation-engine*=validate]").length ? "data-validation-engine" : "class", e.binded && (d.on(e.validationEventTrigger, "[" + e.validateAttribute + "*=validate]:not([type=checkbox]):not([type=radio]):not(.datepicker)", b._onFieldEvent), d.on("click", "[" + e.validateAttribute + "*=validate][type=checkbox],[" + e.validateAttribute + "*=validate][type=radio]", b._onFieldEvent), d.on(e.validationEventTrigger, "[" + e.validateAttribute + "*=validate][class*=datepicker]", { delay: 300 }, b._onFieldEvent)), e.autoPositionUpdate && a(window).bind("resize", { noAnimation: !0, formElem: d }, b.updatePromptsPosition), d.on("click", "a[data-validation-engine-skip], a[class*='validate-skip'], button[data-validation-engine-skip], button[class*='validate-skip'], input[data-validation-engine-skip], input[class*='validate-skip']", b._submitButtonClick), d.removeData("jqv_submitButton"), d.on("submit", b._onSubmitEvent), this }, detach: function () { var c = this, d = c.data("jqv"); return c.find("[" + d.validateAttribute + "*=validate]").not("[type=checkbox]").off(d.validationEventTrigger, b._onFieldEvent), c.find("[" + d.validateAttribute + "*=validate][type=checkbox],[class*=validate][type=radio]").off("click", b._onFieldEvent), c.off("submit", b._onSubmitEvent), c.removeData("jqv"), c.off("click", "a[data-validation-engine-skip], a[class*='validate-skip'], button[data-validation-engine-skip], button[class*='validate-skip'], input[data-validation-engine-skip], input[class*='validate-skip']", b._submitButtonClick), c.removeData("jqv_submitButton"), d.autoPositionUpdate && a(window).off("resize", b.updatePromptsPosition), this }, validate: function () { var e, f, c = a(this), d = null; if (c.is("form") || c.hasClass("validationEngineContainer")) { if (c.hasClass("validating")) return !1; c.addClass("validating"), e = c.data("jqv"), d = b._validateFields(this), setTimeout(function () { c.removeClass("validating") }, 100), d && e.onSuccess ? e.onSuccess() : !d && e.onFailure && e.onFailure() } else c.is("form") || c.hasClass("validationEngineContainer") ? c.removeClass("validating") : (f = c.closest("form, .validationEngineContainer"), e = f.data("jqv") ? f.data("jqv") : a.validationEngine.defaults, d = b._validateField(c, e), d && e.onFieldSuccess ? e.onFieldSuccess() : e.onFieldFailure && e.InvalidFields.length > 0 && e.onFieldFailure()); return e.onValidationComplete ? !!e.onValidationComplete(f, d) : d }, updatePromptsPosition: function (c) { var d, e, f; return c && this == window ? (d = c.data.formElem, e = c.data.noAnimation) : d = a(this.closest("form, .validationEngineContainer")), f = d.data("jqv"), d.find("[" + f.validateAttribute + "*=validate]").not(":disabled").each(function () { var g, h, c = a(this); f.prettySelect && c.is(":hidden") && (c = d.find("#" + f.usePrefix + c.attr("id") + f.useSuffix)), g = b._getPrompt(c), h = a(g).find(".formErrorContent").html(), g && b._updatePrompt(c, a(g), h, void 0, !1, f, e) }), this }, showPrompt: function (a, c, d, e) { var f = this.closest("form, .validationEngineContainer"), g = f.data("jqv"); return g || (g = b._saveOptions(this, g)), d && (g.promptPosition = d), g.showArrow = 1 == e, b._showPrompt(this, a, c, !1, g), this }, hide: function () { var f, c = a(this).closest("form, .validationEngineContainer"), d = c.data("jqv"), e = d && d.fadeDuration ? d.fadeDuration : .3; return f = a(this).is("form") || a(this).hasClass("validationEngineContainer") ? "parentForm" + b._getClassName(a(this).attr("id")) : b._getClassName(a(this).attr("id")) + "formError", a("." + f).fadeTo(e, .3, function () { a(this).parent(".formErrorOuter").remove(), a(this).remove() }), this }, hideAll: function () { var b = this, c = b.data("jqv"), d = c ? c.fadeDuration : 300; return a(".formError").fadeTo(d, 300, function () { a(this).parent(".formErrorOuter").remove(), a(this).remove() }), this }, _onFieldEvent: function (c) { var d = a(this), e = d.closest("form, .validationEngineContainer"), f = e.data("jqv"); f.eventTrigger = "field", window.setTimeout(function () { b._validateField(d, f), 0 == f.InvalidFields.length && f.onFieldSuccess ? f.onFieldSuccess() : f.InvalidFields.length > 0 && f.onFieldFailure && f.onFieldFailure() }, c.data ? c.data.delay : 0) }, _onSubmitEvent: function () { var e, f, c = a(this), d = c.data("jqv"); return c.data("jqv_submitButton") && (e = a("#" + c.data("jqv_submitButton")), e && e.length > 0 && (e.hasClass("validate-skip") || "true" == e.attr("data-validation-engine-skip"))) ? !0 : (d.eventTrigger = "submit", f = b._validateFields(c), f && d.ajaxFormValidation ? (b._validateFormWithAjax(c, d), !1) : d.onValidationComplete ? !!d.onValidationComplete(c, f) : f) }, _checkAjaxStatus: function (b) { var c = !0; return a.each(b.ajaxValidCache, function (a, b) { return b ? void 0 : (c = !1, !1) }), c }, _checkAjaxFieldStatus: function (a, b) { return 1 == b.ajaxValidCache[a] }, _validateFields: function (c) { var f, g, h, i, j, k, l, m, n, d = c.data("jqv"), e = !1; if (c.trigger("jqv.form.validating"), f = null, c.find("[" + d.validateAttribute + "*=validate]").not(":disabled").each(function () { var g = a(this), h = []; if (a.inArray(g.attr("name"), h) < 0) { if (e |= b._validateField(g, d), e && null == f && (g.is(":hidden") && d.prettySelect ? f = g = c.find("#" + d.usePrefix + b._jqSelector(g.attr("id")) + d.useSuffix) : (g.data("jqv-prompt-at") instanceof jQuery ? g = g.data("jqv-prompt-at") : g.data("jqv-prompt-at") && (g = a(g.data("jqv-prompt-at"))), f = g)), d.doNotShowAllErrosOnSubmit) return !1; if (h.push(g.attr("name")), 1 == d.showOneMessage && e) return !1 } }), c.trigger("jqv.form.result", [e]), e) { if (d.scroll) if (g = f.offset().top, h = f.offset().left, i = d.promptPosition, "string" == typeof i && -1 != i.indexOf(":") && (i = i.substring(0, i.indexOf(":"))), "bottomRight" != i && "bottomLeft" != i && (j = b._getPrompt(f), j && (g = j.offset().top)), d.scrollOffset && (g -= d.scrollOffset), d.isOverflown) { if (k = a(d.overflownDIV), !k.length) return !1; l = k.scrollTop(), m = -parseInt(k.offset().top), g += l + m - 5, n = a(d.overflownDIV + ":not(:animated)"), n.animate({ scrollTop: g }, 1100, function () { d.focusFirstField && f.focus() }) } else a("html, body").animate({ scrollTop: g }, 1100, function () { d.focusFirstField && f.focus() }), a("html, body").animate({ scrollLeft: h }, 1100); else d.focusFirstField && f.focus(); return !1 } return !0 }, _validateFormWithAjax: function (c, d) { var e = c.serialize(), f = d.ajaxFormValidationMethod ? d.ajaxFormValidationMethod : "GET", g = d.ajaxFormValidationURL ? d.ajaxFormValidationURL : c.attr("action"), h = d.dataType ? d.dataType : "json"; a.ajax({ type: f, url: g, cache: !1, dataType: h, data: e, form: c, methods: b, options: d, beforeSend: function () { return d.onBeforeAjaxFormValidation(c, d) }, error: function (a, c) { d.onFailure ? d.onFailure(a, c) : b._ajaxError(a, c) }, success: function (e) { var f, g, i, j, k, l, m; if ("json" == h && e !== !0) { for (f = !1, g = 0; g < e.length; g++) i = e[g], j = i[0], k = a(a("#" + j)[0]), 1 == k.length && (l = i[2], 1 == i[1] ? "" != l && l ? (d.allrules[l] && (m = d.allrules[l].alertTextOk, m && (l = m)), d.showPrompts && b._showPrompt(k, l, "pass", !1, d, !0)) : b._closePrompt(k) : (f |= !0, d.allrules[l] && (m = d.allrules[l].alertText, m && (l = m)), d.showPrompts && b._showPrompt(k, l, "", !1, d, !0))); d.onAjaxFormComplete(!f, c, e, d) } else d.onAjaxFormComplete(!0, c, e, d) } }) }, _validateField: function (c, d, e) { var f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z; if (c.attr("id") || (c.attr("id", "form-validation-field-" + a.validationEngine.fieldIdCounter), ++a.validationEngine.fieldIdCounter), !d.validateNonVisibleFields && (c.is(":hidden") && !d.prettySelect || c.parent().is(":hidden"))) return !1; if (f = c.attr(d.validateAttribute), g = /validate\[(.*)\]/.exec(f), !g) return !1; for (h = g[1], i = h.split(/\[|,|\]/), j = !1, k = c.attr("name"), l = "", m = "", n = !1, o = !1, d.isError = !1, d.showArrow = !0, d.maxErrorsPerField > 0 && (o = !0), p = a(c.closest("form, .validationEngineContainer")), q = 0; q < i.length; q++) i[q] = i[q].replace(" ", ""), "" === i[q] && delete i[q]; for (q = 0, r = 0; q < i.length; q++) { if (o && r >= d.maxErrorsPerField) { n || (s = a.inArray("required", i), n = -1 != s && s >= q); break } switch (t = void 0, i[q]) { case "required": n = !0, t = b._getErrorMessage(p, c, i[q], i, q, d, b._required); break; case "custom": t = b._getErrorMessage(p, c, i[q], i, q, d, b._custom); break; case "groupRequired": u = "[" + d.validateAttribute + "*=" + i[q + 1] + "]", v = p.find(u).eq(0), v[0] != c[0] && (b._validateField(v, d, e), d.showArrow = !0), t = b._getErrorMessage(p, c, i[q], i, q, d, b._groupRequired), t && (n = !0), d.showArrow = !1; break; case "ajax": t = b._ajax(c, i, q, d), t && (m = "load"); break; case "minSize": t = b._getErrorMessage(p, c, i[q], i, q, d, b._minSize); break; case "maxSize": t = b._getErrorMessage(p, c, i[q], i, q, d, b._maxSize); break; case "min": t = b._getErrorMessage(p, c, i[q], i, q, d, b._min); break; case "max": t = b._getErrorMessage(p, c, i[q], i, q, d, b._max); break; case "past": t = b._getErrorMessage(p, c, i[q], i, q, d, b._past); break; case "future": t = b._getErrorMessage(p, c, i[q], i, q, d, b._future); break; case "dateRange": u = "[" + d.validateAttribute + "*=" + i[q + 1] + "]", d.firstOfGroup = p.find(u).eq(0), d.secondOfGroup = p.find(u).eq(1), (d.firstOfGroup[0].value || d.secondOfGroup[0].value) && (t = b._getErrorMessage(p, c, i[q], i, q, d, b._dateRange)), t && (n = !0), d.showArrow = !1; break; case "dateTimeRange": u = "[" + d.validateAttribute + "*=" + i[q + 1] + "]", d.firstOfGroup = p.find(u).eq(0), d.secondOfGroup = p.find(u).eq(1), (d.firstOfGroup[0].value || d.secondOfGroup[0].value) && (t = b._getErrorMessage(p, c, i[q], i, q, d, b._dateTimeRange)), t && (n = !0), d.showArrow = !1; break; case "maxCheckbox": c = a(p.find("input[name='" + k + "']")), t = b._getErrorMessage(p, c, i[q], i, q, d, b._maxCheckbox); break; case "minCheckbox": c = a(p.find("input[name='" + k + "']")), t = b._getErrorMessage(p, c, i[q], i, q, d, b._minCheckbox); break; case "equals": t = b._getErrorMessage(p, c, i[q], i, q, d, b._equals); break; case "funcCall": t = b._getErrorMessage(p, c, i[q], i, q, d, b._funcCall); break; case "creditCard": t = b._getErrorMessage(p, c, i[q], i, q, d, b._creditCard); break; case "condRequired": t = b._getErrorMessage(p, c, i[q], i, q, d, b._condRequired), void 0 !== t && (n = !0) } if (w = !1, "object" == typeof t) switch (t.status) { case "_break": w = !0; break; case "_error": t = t.message; break; case "_error_no_prompt": return !0 } if (w) break; "string" == typeof t && (l += t + "<br/>", d.isError = !0, r++) } return !n && !c.val() && c.val().length < 1 && i.indexOf("equals") < 0 && (d.isError = !1), x = c.prop("type"), y = c.data("promptPosition") || d.promptPosition, ("radio" == x || "checkbox" == x) && p.find("input[name='" + k + "']").size() > 1 && (c = "inline" === y ? a(p.find("input[name='" + k + "'][type!=hidden]:last")) : a(p.find("input[name='" + k + "'][type!=hidden]:first")), d.showArrow = !1), c.is(":hidden") && d.prettySelect && (c = p.find("#" + d.usePrefix + b._jqSelector(c.attr("id")) + d.useSuffix)), d.isError && d.showPrompts ? b._showPrompt(c, l, m, !1, d) : j || b._closePrompt(c), j || c.trigger("jqv.field.result", [c, d.isError, l]), z = a.inArray(c[0], d.InvalidFields), -1 == z ? d.isError && d.InvalidFields.push(c[0]) : d.isError || d.InvalidFields.splice(z, 1), b._handleStatusCssClasses(c, d), d.isError && d.onFieldFailure && d.onFieldFailure(c), !d.isError && d.onFieldSuccess && d.onFieldSuccess(c), d.isError }, _handleStatusCssClasses: function (a, b) { b.addSuccessCssClassToField && a.removeClass(b.addSuccessCssClassToField), b.addFailureCssClassToField && a.removeClass(b.addFailureCssClassToField), b.addSuccessCssClassToField && !b.isError && a.addClass(b.addSuccessCssClassToField), b.addFailureCssClassToField && b.isError && a.addClass(b.addFailureCssClassToField) }, _getErrorMessage: function (c, d, e, f, g, h, i) { var k, l, m, n, o, p, j = jQuery.inArray(e, f); return ("custom" === e || "funcCall" === e) && (k = f[j + 1], e = e + "[" + k + "]", delete f[j]), l = e, m = d.attr("data-validation-engine") ? d.attr("data-validation-engine") : d.attr("class"), n = m.split(" "), o = "future" == e || "past" == e || "maxCheckbox" == e || "minCheckbox" == e ? i(c, d, f, g, h) : i(d, f, g, h), void 0 != o && (p = b._getCustomErrorMessage(a(d), n, l, h), p && (o = p)), o }, _getCustomErrorMessage: function (a, c, d, e) { var h, i, j, f = !1, g = /^custom\[.*\]$/.test(d) ? b._validityProp.custom : b._validityProp[d]; if (void 0 != g && (f = a.attr("data-errormessage-" + g), void 0 != f)) return f; if (f = a.attr("data-errormessage"), void 0 != f) return f; if (h = "#" + a.attr("id"), "undefined" != typeof e.custom_error_messages[h] && "undefined" != typeof e.custom_error_messages[h][d]) f = e.custom_error_messages[h][d].message; else if (c.length > 0) for (i = 0; i < c.length && c.length > 0; i++) if (j = "." + c[i], "undefined" != typeof e.custom_error_messages[j] && "undefined" != typeof e.custom_error_messages[j][d]) { f = e.custom_error_messages[j][d].message; break } return f || "undefined" == typeof e.custom_error_messages[d] || "undefined" == typeof e.custom_error_messages[d].message || (f = e.custom_error_messages[d].message), f }, _validityProp: { required: "value-missing", custom: "custom-error", groupRequired: "value-missing", ajax: "custom-error", minSize: "range-underflow", maxSize: "range-overflow", min: "range-underflow", max: "range-overflow", past: "type-mismatch", future: "type-mismatch", dateRange: "type-mismatch", dateTimeRange: "type-mismatch", maxCheckbox: "range-overflow", minCheckbox: "range-underflow", equals: "pattern-mismatch", funcCall: "custom-error", creditCard: "pattern-mismatch", condRequired: "value-missing" }, _required: function (b, c, d, e, f) { var g, h, i, j, k; switch (b.prop("type")) { case "text": case "password": case "textarea": case "file": case "select-one": case "select-multiple": default: if (g = a.trim(b.val()), h = a.trim(b.attr("data-validation-placeholder")), i = a.trim(b.attr("placeholder")), !g || h && g == h || i && g == i) return e.allrules[c[d]].alertText; break; case "radio": case "checkbox": if (f) { if (!b.attr("checked")) return e.allrules[c[d]].alertTextCheckboxMultiple; break } if (j = b.closest("form, .validationEngineContainer"), k = b.attr("name"), 0 == j.find("input[name='" + k + "']:checked").size()) return 1 == j.find("input[name='" + k + "']:visible").size() ? e.allrules[c[d]].alertTextCheckboxe : e.allrules[c[d]].alertTextCheckboxMultiple } }, _groupRequired: function (c, d, e, f) { var g = "[" + f.validateAttribute + "*=" + d[e + 1] + "]", h = !1; return c.closest("form, .validationEngineContainer").find(g).each(function () { return b._required(a(this), d, e, f) ? void 0 : (h = !0, !1) }), h ? void 0 : f.allrules[d[e]].alertText }, _custom: function (a, b, c, d) { var g, h, i, e = b[c + 1], f = d.allrules[e]; if (!f) return alert("jqv:custom rule not found - " + e), void 0; if (f.regex) { if (h = f.regex, !h) return alert("jqv:custom regex not found - " + e), void 0; if (i = new RegExp(h), !i.test(a.val())) return d.allrules[e].alertText } else { if (!f.func) return alert("jqv:custom type not allowed " + e), void 0; if (g = f.func, "function" != typeof g) return alert("jqv:custom parameter 'function' is no function - " + e), void 0; if (!g(a, b, c, d)) return d.allrules[e].alertText } }, _funcCall: function (a, b, c, d) { var f, g, h, e = b[c + 1]; if (e.indexOf(".") > -1) { for (g = e.split("."), h = window; g.length;) h = h[g.shift()]; f = h } else f = window[e] || d.customFunctions[e]; return "function" == typeof f ? f(a, b, c, d) : void 0 }, _equals: function (b, c, d, e) { var f = c[d + 1]; return b.val() != a("#" + f).val() ? e.allrules.equals.alertText : void 0 }, _maxSize: function (a, b, c, d) { var g, e = b[c + 1], f = a.val().length; return f > e ? (g = d.allrules.maxSize, g.alertText + e + g.alertText2) : void 0 }, _minSize: function (a, b, c, d) { var g, e = b[c + 1], f = a.val().length; return e > f ? (g = d.allrules.minSize, g.alertText + e + g.alertText2) : void 0 }, _min: function (a, b, c, d) { var g, e = parseFloat(b[c + 1]), f = parseFloat(a.val()); return e > f ? (g = d.allrules.min, g.alertText2 ? g.alertText + e + g.alertText2 : g.alertText + e) : void 0 }, _max: function (a, b, c, d) { var g, e = parseFloat(b[c + 1]), f = parseFloat(a.val()); return f > e ? (g = d.allrules.max, g.alertText2 ? g.alertText + e + g.alertText2 : g.alertText + e) : void 0 }, _past: function (c, d, e, f, g) { var j, k, l, h = e[f + 1], i = a(c.find("*[name='" + h.replace(/^#+/, "") + "']")); if ("now" == h.toLowerCase()) j = new Date; else if (void 0 != i.val()) { if (i.is(":disabled")) return; j = b._parseDate(i.val()) } else j = b._parseDate(h); return k = b._parseDate(d.val()), k > j ? (l = g.allrules.past, l.alertText2 ? l.alertText + b._dateToString(j) + l.alertText2 : l.alertText + b._dateToString(j)) : void 0 }, _future: function (c, d, e, f, g) { var j, k, l, h = e[f + 1], i = a(c.find("*[name='" + h.replace(/^#+/, "") + "']")); if ("now" == h.toLowerCase()) j = new Date; else if (void 0 != i.val()) { if (i.is(":disabled")) return; j = b._parseDate(i.val()) } else j = b._parseDate(h); return k = b._parseDate(d.val()), j > k ? (l = g.allrules.future, l.alertText2 ? l.alertText + b._dateToString(j) + l.alertText2 : l.alertText + b._dateToString(j)) : void 0 }, _isDate: function (a) { var b = new RegExp(/^\d{4}[\/\-](0?[1-9]|1[012])[\/\-](0?[1-9]|[12][0-9]|3[01])$|^(?:(?:(?:0?[13578]|1[02])(\/|-)31)|(?:(?:0?[1,3-9]|1[0-2])(\/|-)(?:29|30)))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^(?:(?:0?[1-9]|1[0-2])(\/|-)(?:0?[1-9]|1\d|2[0-8]))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^(0?2(\/|-)29)(\/|-)(?:(?:0[48]00|[13579][26]00|[2468][048]00)|(?:\d\d)?(?:0[48]|[2468][048]|[13579][26]))$/); return b.test(a) }, _isDateTime: function (a) { var b = new RegExp(/^\d{4}[\/\-](0?[1-9]|1[012])[\/\-](0?[1-9]|[12][0-9]|3[01])\s+(1[012]|0?[1-9]){1}:(0?[1-5]|[0-6][0-9]){1}:(0?[0-6]|[0-6][0-9]){1}\s+(am|pm|AM|PM){1}$|^(?:(?:(?:0?[13578]|1[02])(\/|-)31)|(?:(?:0?[1,3-9]|1[0-2])(\/|-)(?:29|30)))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^((1[012]|0?[1-9]){1}\/(0?[1-9]|[12][0-9]|3[01]){1}\/\d{2,4}\s+(1[012]|0?[1-9]){1}:(0?[1-5]|[0-6][0-9]){1}:(0?[0-6]|[0-6][0-9]){1}\s+(am|pm|AM|PM){1})$/); return b.test(a) }, _dateCompare: function (a, b) { return new Date(a.toString()) < new Date(b.toString()) }, _dateRange: function (a, c, d, e) { return !e.firstOfGroup[0].value && e.secondOfGroup[0].value || e.firstOfGroup[0].value && !e.secondOfGroup[0].value ? e.allrules[c[d]].alertText + e.allrules[c[d]].alertText2 : b._isDate(e.firstOfGroup[0].value) && b._isDate(e.secondOfGroup[0].value) ? b._dateCompare(e.firstOfGroup[0].value, e.secondOfGroup[0].value) ? void 0 : e.allrules[c[d]].alertText + e.allrules[c[d]].alertText2 : e.allrules[c[d]].alertText + e.allrules[c[d]].alertText2 }, _dateTimeRange: function (a, c, d, e) { return !e.firstOfGroup[0].value && e.secondOfGroup[0].value || e.firstOfGroup[0].value && !e.secondOfGroup[0].value ? e.allrules[c[d]].alertText + e.allrules[c[d]].alertText2 : b._isDateTime(e.firstOfGroup[0].value) && b._isDateTime(e.secondOfGroup[0].value) ? b._dateCompare(e.firstOfGroup[0].value, e.secondOfGroup[0].value) ? void 0 : e.allrules[c[d]].alertText + e.allrules[c[d]].alertText2 : e.allrules[c[d]].alertText + e.allrules[c[d]].alertText2 }, _maxCheckbox: function (a, b, c, d, e) { var f = c[d + 1], g = b.attr("name"), h = a.find("input[name='" + g + "']:checked").size(); return h > f ? (e.showArrow = !1, e.allrules.maxCheckbox.alertText2 ? e.allrules.maxCheckbox.alertText + " " + f + " " + e.allrules.maxCheckbox.alertText2 : e.allrules.maxCheckbox.alertText) : void 0 }, _minCheckbox: function (a, b, c, d, e) { var f = c[d + 1], g = b.attr("name"), h = a.find("input[name='" + g + "']:checked").size(); return f > h ? (e.showArrow = !1, e.allrules.minCheckbox.alertText + " " + f + " " + e.allrules.minCheckbox.alertText2) : void 0 }, _creditCard: function (a, b, c, d) { var j, h, i, k, e = !1, f = a.val().replace(/ +/g, "").replace(/-+/g, ""), g = f.length; if (g >= 14 && 16 >= g && parseInt(f) > 0) { h = 0, c = g - 1, i = 1, k = new String; do j = parseInt(f.charAt(c)), k += 0 == i++ % 2 ? 2 * j : j; while (--c >= 0); for (c = 0; c < k.length; c++) h += parseInt(k.charAt(c)); e = 0 == h % 10 } return e ? void 0 : d.allrules.creditCard.alertText }, _ajax: function (c, d, e, f) { var l, m, o, p, q, g = d[e + 1], h = f.allrules[g], i = h.extraData, j = h.extraDataDynamic, k = { fieldId: c.attr("id"), fieldValue: c.val() }; if ("object" == typeof i) a.extend(k, i); else if ("string" == typeof i) for (l = i.split("&"), e = 0; e < l.length; e++) m = l[e].split("="), m[0] && m[0] && (k[m[0]] = m[1]); if (j) for (o = String(j).split(","), e = 0; e < o.length; e++) p = o[e], a(p).length && (q = c.closest("form, .validationEngineContainer").find(p).val(), p.replace("#", "") + "=" + escape(q), k[p.replace("#", "")] = q); return "field" == f.eventTrigger && delete f.ajaxValidCache[c.attr("id")], f.isError || b._checkAjaxFieldStatus(c.attr("id"), f) ? void 0 : (a.ajax({ type: f.ajaxFormValidationMethod, url: h.url, cache: !1, dataType: "json", data: k, field: c, rule: h, methods: b, options: f, beforeSend: function () { }, error: function (a, c) { f.onFailure ? f.onFailure(a, c) : b._ajaxError(a, c) }, success: function (d) { var i, j, k, e = d[0], g = a("#" + e).eq(0); 1 == g.length && (i = d[1], j = d[2], i ? (f.ajaxValidCache[e] = !0, j ? f.allrules[j] && (k = f.allrules[j].alertTextOk, k && (j = k)) : j = h.alertTextOk, f.showPrompts && (j ? b._showPrompt(g, j, "pass", !0, f) : b._closePrompt(g)), "submit" == f.eventTrigger && c.closest("form").submit()) : (f.ajaxValidCache[e] = !1, f.isError = !0, j ? f.allrules[j] && (k = f.allrules[j].alertText, k && (j = k)) : j = h.alertText, f.showPrompts && b._showPrompt(g, j, "", !0, f))), g.trigger("jqv.field.result", [g, f.isError, j]) } }), h.alertTextLoad) }, _ajaxError: function (a, b) { 0 == a.status && null == b ? alert("The page is not served from a server! ajax call failed") : "undefined" != typeof console && console.log("Ajax error: " + a.status + " " + b) }, _dateToString: function (a) { return a.getFullYear() + "-" + (a.getMonth() + 1) + "-" + a.getDate() }, _parseDate: function (a) { var b = a.split("-"); return b == a && (b = a.split("/")), b == a ? (b = a.split("."), new Date(b[2], b[1] - 1, b[0])) : new Date(b[0], b[1] - 1, b[2]) }, _showPrompt: function (c, d, e, f, g, h) { c.data("jqv-prompt-at") instanceof jQuery ? c = c.data("jqv-prompt-at") : c.data("jqv-prompt-at") && (c = a(c.data("jqv-prompt-at"))); var i = b._getPrompt(c); h && (i = !1), a.trim(d) && (i ? b._updatePrompt(c, i, d, e, f, g) : b._buildPrompt(c, d, e, f, g)) }, _buildPrompt: function (c, d, e, f, g) { var j, k, l, m, n, h = a("<div>"); switch (h.addClass(b._getClassName(c.attr("id")) + "formError"), h.addClass("parentForm" + b._getClassName(c.closest("form, .validationEngineContainer").attr("id"))), h.addClass("formError"), e) { case "pass": h.addClass("greenPopup"); break; case "load": h.addClass("blackPopup") } if (f && h.addClass("ajaxed"), a("<div>").addClass("formErrorContent").html(d).appendTo(h), j = c.data("promptPosition") || g.promptPosition, g.showArrow) switch (k = a("<div>").addClass("formErrorArrow"), "string" == typeof j && (l = j.indexOf(":"), -1 != l && (j = j.substring(0, l))), j) { case "bottomLeft": case "bottomRight": h.find(".formErrorContent").before(k), k.addClass("formErrorArrowBottom").html('<div class="line1"><!-- --></div><div class="line2"><!-- --></div><div class="line3"><!-- --></div><div class="line4"><!-- --></div><div class="line5"><!-- --></div><div class="line6"><!-- --></div><div class="line7"><!-- --></div><div class="line8"><!-- --></div><div class="line9"><!-- --></div><div class="line10"><!-- --></div>'); break; case "topLeft": case "topRight": k.html('<div class="line10"><!-- --></div><div class="line9"><!-- --></div><div class="line8"><!-- --></div><div class="line7"><!-- --></div><div class="line6"><!-- --></div><div class="line5"><!-- --></div><div class="line4"><!-- --></div><div class="line3"><!-- --></div><div class="line2"><!-- --></div><div class="line1"><!-- --></div>'), h.append(k) } return g.addPromptClass && h.addClass(g.addPromptClass), m = c.attr("data-required-class"), void 0 !== m ? h.addClass(m) : g.prettySelect && a("#" + c.attr("id")).next().is("select") && (n = a("#" + c.attr("id").substr(g.usePrefix.length).substring(g.useSuffix.length)).attr("data-required-class"), void 0 !== n && h.addClass(n)), h.css({ opacity: 0 }), "inline" === j ? (h.addClass("inline"), "undefined" != typeof c.attr("data-prompt-target") && a("#" + c.attr("data-prompt-target")).length > 0 ? h.appendTo(a("#" + c.attr("data-prompt-target"))) : c.after(h)) : c.before(h), l = b._calculatePosition(c, h, g), h.css({ position: "inline" === j ? "relative" : "absolute", top: l.callerTopPosition, left: l.callerleftPosition, marginTop: l.marginTopSize, opacity: 0 }).data("callerField", c), g.autoHidePrompt && setTimeout(function () { h.animate({ opacity: 0 }, function () { h.closest(".formErrorOuter").remove(), h.remove() }) }, g.autoHideDelay), h.animate({ opacity: .87 }) }, _updatePrompt: function (a, c, d, e, f, g, h) { var i, j; c && ("undefined" != typeof e && ("pass" == e ? c.addClass("greenPopup") : c.removeClass("greenPopup"), "load" == e ? c.addClass("blackPopup") : c.removeClass("blackPopup")), f ? c.addClass("ajaxed") : c.removeClass("ajaxed"), c.find(".formErrorContent").html(d), i = b._calculatePosition(a, c, g), j = { top: i.callerTopPosition, left: i.callerleftPosition, marginTop: i.marginTopSize }, h ? c.css(j) : c.animate(j)) }, _closePrompt: function (a) { var c = b._getPrompt(a); c && c.fadeTo("fast", 0, function () { c.parent(".formErrorOuter").remove(), c.remove() }) }, closePrompt: function (a) { return b._closePrompt(a) }, _getPrompt: function (c) { var d = a(c).closest("form, .validationEngineContainer").attr("id"), e = b._getClassName(c.attr("id")) + "formError", f = a("." + b._escapeExpression(e) + ".parentForm" + b._getClassName(d))[0]; return f ? a(f) : void 0 }, _escapeExpression: function (a) { return a.replace(/([#;&,\.\+\*\~':"\!\^$\[\]\(\)=>\|])/g, "\\$1") }, isRTL: function (b) { var c = a(document), d = a("body"), e = b && b.hasClass("rtl") || b && "rtl" === (b.attr("dir") || "").toLowerCase() || c.hasClass("rtl") || "rtl" === (c.attr("dir") || "").toLowerCase() || d.hasClass("rtl") || "rtl" === (d.attr("dir") || "").toLowerCase(); return Boolean(e) }, _calculatePosition: function (a, b, c) { var d, e, f, k, l, m, n, o, p, g = a.width(), h = a.position().left, i = a.position().top; switch (a.height(), k = b.height(), d = e = 0, f = -k, l = a.data("promptPosition") || c.promptPosition, m = "", n = "", o = 0, p = 0, "string" == typeof l && -1 != l.indexOf(":") && (m = l.substring(l.indexOf(":") + 1), l = l.substring(0, l.indexOf(":")), -1 != m.indexOf(",") && (n = m.substring(m.indexOf(",") + 1), m = m.substring(0, m.indexOf(",")), p = parseInt(n), isNaN(p) && (p = 0)), o = parseInt(m), isNaN(m) && (m = 0)), l) { default: case "topRight": e += h + g - 30, d += i; break; case "topLeft": d += i, e += h; break; case "centerRight": d = i, f = 0, e = h + a.outerWidth(!0) + 5; break; case "centerLeft": e = h - b.outerWidth(), d = i, f = 0; break; case "bottomLeft": d = i + a.outerHeight(), f = 0, e = h; break; case "bottomRight": e = h + g - 30, d = i + a.outerHeight(), f = 0; break; case "inline": e = 0, d = 0, f = 0 } return e += o, d += p, { callerTopPosition: d + "px", callerleftPosition: e + "px", marginTopSize: f + "px" } }, _saveOptions: function (b, c) { var d, e; return a.validationEngineLanguage ? d = a.validationEngineLanguage.allRules : a.error("jQuery.validationEngine rules are not loaded, plz add localization files to the page"), a.validationEngine.defaults.allrules = d, e = a.extend(!0, {}, a.validationEngine.defaults, c), b.data("jqv", e), e }, _getClassName: function (a) { return a ? a.replace(/:/g, "_").replace(/\./g, "_") : void 0 }, _jqSelector: function (a) { return a.replace(/([;&,\.\+\*\~':"\!\^#$%@\[\]\(\)=>\|])/g, "\\$1") }, _condRequired: function (a, c, d, e) { var f, g; for (f = d + 1; f < c.length; f++) if (g = jQuery("#" + c[f]).first(), g.length && void 0 == b._required(g, ["required"], 0, e, !0)) return b._required(a, ["required"], 0, e) }, _submitButtonClick: function () { var c = a(this), d = c.closest("form, .validationEngineContainer"); d.data("jqv_submitButton", c.attr("id")) } }; a.fn.validationEngine = function (c) { var d = a(this); return d[0] ? "string" == typeof c && "_" != c.charAt(0) && b[c] ? ("showPrompt" != c && "hide" != c && "hideAll" != c && b.init.apply(d), b[c].apply(d, Array.prototype.slice.call(arguments, 1))) : "object" != typeof c && c ? (a.error("Method " + c + " does not exist in jQuery.validationEngine"), void 0) : (b.init.apply(d, arguments), b.attach.apply(d)) : d }, a.validationEngine = { fieldIdCounter: 0, defaults: { validationEventTrigger: "blur", scroll: !0, focusFirstField: !0, showPrompts: !0, validateNonVisibleFields: !1, promptPosition: "topRight", bindMethod: "bind", inlineAjax: !1, ajaxFormValidation: !1, ajaxFormValidationURL: !1, ajaxFormValidationMethod: "get", onAjaxFormComplete: a.noop, onBeforeAjaxFormValidation: a.noop, onValidationComplete: !1, doNotShowAllErrosOnSubmit: !1, custom_error_messages: {}, binded: !0, showArrow: !0, isError: !1, maxErrorsPerField: !1, ajaxValidCache: {}, autoPositionUpdate: !1, InvalidFields: [], onFieldSuccess: !1, onFieldFailure: !1, onSuccess: !1, onFailure: !1, validateAttribute: "class", addSuccessCssClassToField: "", addFailureCssClassToField: "", autoHidePrompt: !1, autoHideDelay: 1e4, fadeDuration: .3, prettySelect: !1, addPromptClass: "", usePrefix: "", useSuffix: "", showOneMessage: !1 } }, a(function () { a.validationEngine.defaults.promptPosition = b.isRTL() ? "topLeft" : "topRight" }) }(jQuery);

(function ($) {
    // 验证规则
    $.fn.validationEngineLanguage = function () { };
    $.validationEngineLanguage = {
        newLang: function () {
            $.validationEngineLanguage.allRules = {
                "required": { // Add your regex rules here, you can take telephone as an example
                    "regex": "none",
                    "alertText": "* 必填",
                    "alertTextCheckboxMultiple": "* 请选择一个项目",
                    "alertTextCheckboxe": "* 必选",
                    "alertTextDateRange": "* 日期范围不可空白"
                },
                "dateRange": {
                    "regex": "none",
                    "alertText": "* 无效的 ",
                    "alertText2": " 日期范围"
                },
                "dateTimeRange": {
                    "regex": "none",
                    "alertText": "* 无效的 ",
                    "alertText2": " 时间范围"
                },
                "minSize": {
                    "regex": "none",
                    "alertText": "* 最少 ",
                    "alertText2": " 个字符"
                },
                "maxSize": {
                    "regex": "none",
                    "alertText": "* 最多 ",
                    "alertText2": " 个字符"
                },
                "groupRequired": {
                    "regex": "none",
                    "alertText": "* 至少填写其中一项"
                },
                "min": {
                    "regex": "none",
                    "alertText": "* 最小值为 "
                },
                "max": {
                    "regex": "none",
                    "alertText": "* 最大值为 "
                },
                "past": {
                    "regex": "none",
                    "alertText": "* 日期需在 ",
                    "alertText2": " 之前"
                },
                "future": {
                    "regex": "none",
                    "alertText": "* 日期需在 ",
                    "alertText2": " 之后"
                },
                "maxCheckbox": {
                    "regex": "none",
                    "alertText": "* 最多选择 ",
                    "alertText2": " 个项目"
                },
                "minCheckbox": {
                    "regex": "none",
                    "alertText": "* 最少选择 ",
                    "alertText2": " 个项目"
                },
                "equals": {
                    "regex": "none",
                    "alertText": "* 两次输入的密码不一致"
                },
                "creditCard": {
                    "regex": "none",
                    "alertText": "* 无效的信用卡号码"
                },
                "phone": {
                    // credit:jquery.h5validate.js / orefalo
                    "regex": /^([\+][0-9]{1,3}[ \.\-])?([\(]{1}[0-9]{2,6}[\)])?([0-9 \.\-\/]{3,20})((x|ext|extension)[ ]?[0-9]{1,4})?$/,
                    "alertText": "* 无效的电话号码"
                },
                "MobilePhone": {
                    "regex": /^[1][3,4,5,7,8,9][0-9]{9}$/,
                    "alertText":"*无效的手机号码"
                },
                "WorkShopCode": {
                    "regex": /[A-Za-z]/,
                    "alertText": "*无效的车间代码"
                },
                "email": {
                    // Shamelessly lifted from Scott Gonzalez via the Bassistance Validation plugin http://projects.scottsplayground.com/email_address_validation/
                    "regex": /^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/i,
                    "alertText": "* 无效的邮件地址"
                },
                "integer": {
                    "regex": /^[\-\+]?\d+$/,
                    "alertText": "* 必须输入整数"
                },
                "number": {
                    // Number, including positive, negative, and floating decimal. credit:orefalo
                    "regex": /^[\-\+]?((([0-9]{1,3})([,][0-9]{3})*)|([0-9]+))?([\.]([0-9]+))?$/,
                    "alertText": "* 无效的数值"
                },
                "date": {
                    "regex": /^\d{4}[\/\-](0?[1-9]|1[012])[\/\-](0?[1-9]|[12][0-9]|3[01])$/,
                    "alertText": "* 无效的日期，格式必需为 YYYY-MM-DD"
                },
                "ipv4": {
                    "regex": /^((([01]?[0-9]{1,2})|(2[0-4][0-9])|(25[0-5]))[.]){3}(([0-1]?[0-9]{1,2})|(2[0-4][0-9])|(25[0-5]))$/,
                    "alertText": "* 无效的 IP 地址"
                },
                "url": {
                    "regex": /^(https?|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$/i,
                    "alertText": "* 无效的网址,示例：http://www.baidu.com"
                },
                "onlyNumberSp": {
                    "regex": /^[0-9\ ]+$/,
                    "alertText": "* 只能填写数字"
                },
                "onlyLetterSp": {
                    "regex": /^[a-zA-Z\ \']+$/,
                    "alertText": "* 只能填写英文字母"
                },
                "onlyLetterNumber": {
                    "regex": /^[0-9a-zA-Z]+$/,
                    "alertText": "* 只能填写数字与英文字母"
                },
                //tls warning:homegrown not fielded 
                "dateFormat": {
                    "regex": /^\d{4}[\/\-](0?[1-9]|1[012])[\/\-](0?[1-9]|[12][0-9]|3[01])$|^(?:(?:(?:0?[13578]|1[02])(\/|-)31)|(?:(?:0?[1,3-9]|1[0-2])(\/|-)(?:29|30)))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^(?:(?:0?[1-9]|1[0-2])(\/|-)(?:0?[1-9]|1\d|2[0-8]))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^(0?2(\/|-)29)(\/|-)(?:(?:0[48]00|[13579][26]00|[2468][048]00)|(?:\d\d)?(?:0[48]|[2468][048]|[13579][26]))$/,
                    "alertText": "* 无效的日期格式"
                },
                //tls warning:homegrown not fielded 
                "dateTimeFormat": {
                    "regex": /^\d{4}[\/\-](0?[1-9]|1[012])[\/\-](0?[1-9]|[12][0-9]|3[01])\s+(1[012]|0?[1-9]){1}:(0?[1-5]|[0-6][0-9]){1}:(0?[0-6]|[0-6][0-9]){1}\s+(am|pm|AM|PM){1}$|^(?:(?:(?:0?[13578]|1[02])(\/|-)31)|(?:(?:0?[1,3-9]|1[0-2])(\/|-)(?:29|30)))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^((1[012]|0?[1-9]){1}\/(0?[1-9]|[12][0-9]|3[01]){1}\/\d{2,4}\s+(1[012]|0?[1-9]){1}:(0?[1-5]|[0-6][0-9]){1}:(0?[0-6]|[0-6][0-9]){1}\s+(am|pm|AM|PM){1})$/,
                    "alertText": "* 无效的日期或时间格式",
                    "alertText2": "可接受的格式： ",
                    "alertText3": "mm/dd/yyyy hh:mm:ss AM|PM 或 ",
                    "alertText4": "yyyy-mm-dd hh:mm:ss AM|PM"
                },
                // 自定义规则示例，提供参考，可删除
                "validate2fields": {
                    "alertText": "* 请输入 HELLO"
                },
                "requiredInFunction": {
                    "func": function (field, rules, i, options) {
                        return (field.val() == "test") ? true : false;
                    },
                    "alertText": "* 必须输入 test"
                },
                "ajaxUserCall": {
                    "url": "ajaxValidateFieldUser",
                    // you may want to pass extra data on the ajax call
                    "extraData": "name=eric",
                    "alertText": "* 此名称已被其他人使用",
                    "alertTextLoad": "* 正在确认名称是否有其他人使用，请稍等。"
                },
                "ajaxUserCallPhp": {
                    "url": "phpajax/ajaxValidateFieldUser.php",
                    // you may want to pass extra data on the ajax call
                    "extraData": "name=eric",
                    // if you provide an "alertTextOk", it will show as a green prompt when the field validates
                    "alertTextOk": "* 此帐号名称可以使用",
                    "alertText": "* 此名称已被其他人使用",
                    "alertTextLoad": "* 正在确认帐号名称是否有其他人使用，请稍等。"
                },
                "ajaxNameCall": {
                    // remote json service location
                    "url": "ajaxValidateFieldName",
                    // error
                    "alertText": "* 此名称可以使用",
                    // if you provide an "alertTextOk", it will show as a green prompt when the field validates
                    "alertTextOk": "* 此名称已被其他人使用",
                    // speaks by itself
                    "alertTextLoad": "* 正在确认名称是否有其他人使用，请稍等。"
                },
                "ajaxNameCallPhp": {
                    // remote json service location
                    "url": "phpajax/ajaxValidateFieldName.php",
                    // error
                    "alertText": "* 此名称已被其他人使用",
                    // speaks by itself
                    "alertTextLoad": "* 正在确认名称是否有其他人使用，请稍等。"
                },
                "ajaxCheckCompanyName": {
                    'url': '/ajax/ajax.aspx?action=ajaxCheckCompanyName', /* 验证程序地址 */
                    'type': 'Post',
                    'alertTextOk': '该公司未被使用，可以继续注册',
                    'alertText': '该公司名己被使用，不能继续注册',
                    'alertTextLoad': '正在验证是否己被注册'
                }, "ajaxCheckCompanyMobilePhone": {
                    'url': '/ajax/ajax.aspx?action=ajaxCheckCompanyMobilePhone', /* 验证程序地址 */
                    'type': 'Post',
                    'alertTextOk': '该手机号未被使用，可以继续注册',
                    'alertText': '该手机号己被使用，不能继续注册',
                    'alertTextLoad': '正在验证是否己被注册'
                }, "ajaxCheckUserMobilePhone": {
                    'url': '/ajax/ajax.aspx?action=ajaxCheckUserMobilePhone', /* 验证程序地址 */
                    'type': 'Post',
                    'alertTextOk': '该手机号未被使用，可以继续注册',
                    'alertText': '该手机号己被使用，不能继续注册',
                    'alertTextLoad': '正在验证是否己被注册'
                }
            };

        }
    };
    $.validationEngineLanguage.newLang();
})(jQuery);