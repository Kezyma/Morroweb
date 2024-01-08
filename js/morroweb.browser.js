
//#region Constants
const mw_specialisationMap = ["combat", "magic", "stealth"]
const mw_attributeMap = ["strength", "intelligence", "willpower", "agility", "speed", "endurance", "personality", "luck"];
//#endregion

var entityMap = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;',
    '/': '&#x2F;',
    '`': '&#x60;',
    '=': '&#x3D;'
};

function escapeHtml(string) {
    return String(string).replace(/[&<>"'`=\/]/g, function (s) {
        return entityMap[s];
    });
}

//#region Pre-Load
var lookupInit = false
function mw_preLoad() {
    $.get({
        url: "json/morroweb.gmst.json",
        cache: true,
        success: function (data) {
            for (var s in data) {
                mw_gmst_lookup[s.toLowerCase()] = data[s]
            }
        }
    });
    $.get({
        url: "json/morroweb.lookup.json",
        cache: true,
        success: function (data) {
            mw_effectIcon_lookup = data["magiceffecticon"]
            mw_spell_lookup = data["spell"]
            for (var spell in data["spelleffect"]) {
                mw_spellIcon_lookup[spell] = mw_effectIcon_lookup[data["spelleffect"][spell]]
            }
        }
    });
}
var mw_gmst_lookup = {}
var mw_effectIcon_lookup = {}
var mw_spell_lookup = {}
var mw_spellIcon_lookup = {}
var currentTable = null;
var allTables = {}

$(document).ready(function () {
    mw_preLoad();
});

function mw_resize() {
    $(window).trigger("resize");
}
//#endregion

//#region Table Functions
function mw_newTable(id) {
    return $(`<table id='mw_${id}_table' class='mw-table table-sm table-borderless display table'></table>`);
}

function mw_newDataTable(container, id, headings, rows, filterWidths) {
    // Generate the DataTable.
    var table = mw_newTable(id);
    var cardBody = $(`#${container}`);
    cardBody.append(table);
    var newTable = new DataTable(`#mw_${id}_table`, {
        columns: headings,
        data: rows,
        paging: false,
        scrollCollapse: true,
        scrollY: true,
        scrollY: "calc(100vh - 18rem)",
        scrollX: true,
        sScrollX: "100%",
        initComplete: function (settings) {
            var preFilters = {}
            var ft = $("<tr></tr>");
            for (var i in headings) {
                var w = 1;
                if (filterWidths != null && filterWidths.length > i) {
                    w = filterWidths[i]
                }
                // Check if this filter has a value in the querystring.
                var qs = new URL(location.href).searchParams.get(i);
                if (qs != null && qs != undefined && qs != "") {
                    preFilters[i] = qs;
                }

                // Create a new filter.
                ft.append($(`<th>${mw_textFilter(i, id, w)}</th>`))
            }
            $(settings.nTHead).append(ft);

            // Set any values from the querystring.
            for (var i in preFilters) {
                $(`input[data-table=${id}][data-ix=${i}]`).val(preFilters[i]).change();
            }

            // Resizing hack to fix headings rendering wrong.
            mw_resize();
            setTimeout(mw_resize, 1000);
            
        }
    });
    allTables[container] = newTable;
    currentTable = newTable;

    // Handle filtering.
    $(currentTable.table().container()).on('keyup', 'thead input', function () {
        mw_filterColumn(this);
    });

    // Initialise Bootstrap tooltips.
    var tooltipTriggerList = $(`#mw_${id}_table [data-bs-toggle="tooltip"]`);
    [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))


    var popoverTriggerList = $(`#mw_${id}_table [data-bs-toggle="popover"]`);
    [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl))

    // Apply current filters.
    mw_applyFilters(container);
}

function mw_applyFilters(id) {
    var query = new URL(location.href.split("&")[0]);
    for (var i = 0; i < $(`#${id} .dataTables_scrollHead .mw-table-filter`).length; i++) {
        var input = $(`#${id} .dataTables_scrollHead .mw-table-filter`)[i]
        var val = $(input).val()
        var ix = $(input).data("ix")
        if (ix != undefined) {
            currentTable.column(ix).search(val).draw()
            if (val != "") {
                query.searchParams.set(ix, val)
            }
        }
    }
    window.history.pushState({ title: "Morroweb", html: window.html, id: val }, null, query.href);
}

function mw_filterColumn(input) {
    var query = new URL(location.href);
    var p = $(input).data('ix');
    var v = $(input).val()
    if (v != "") {
        query.searchParams.set(p, v)
    }
    else {
        query.searchParams.delete(p)
    }
    window.history.pushState({ title: "Morroweb", html: window.html, id: v }, null, query.href);

    currentTable
        .column($(input).data('ix'))
        .search(input.value)
        .draw();
}

function mw_showPanel(container) {
    $(".mwb-panel").hide();
    $(`#${container}`).show();
}
//#endregion

//#region Icon Generators
function mw_attribute_icon(attribute="") {
    var a = attribute.toLowerCase().trim()
    if (a != "" && a != "none") {
        return mw_icon(`attribute/attribute_${a}.png`, mw_titleCase(attribute))
    }
    return "";
}

function mw_skill_icon(skill = "") {
    var a = skill.toLowerCase().trim()
    if (a != "" && a != "none") {
        return mw_icon(`skill/${a}.png`, mw_titleCase(mw_gmst_lookup["sskill" + skill.toLowerCase()]))
    }
    return "";
}

function mw_item_icon(icon = "", name="") {
    var a = icon.toLowerCase().trim()
    if (a != "" && a != "none") {
        return mw_icon(`item/${a}.png`, mw_titleCase(name))
    }
    return "";
}

function mw_magicEffect_icon(icon = "", title = "") {
    if (icon != "" && icon != "none") {
        return mw_icon(`magiceffect/b_${icon.toLowerCase()}.png`, title)
    }
    return "";
}

function mw_icon(url, title) {
    return `<img src='img/icons/${url}' data-bs-toggle="tooltip" data-bs-custom-class="mw-tooltip" data-bs-title='${escapeHtml(title)}' />`
}

function mw_description(description) {
    if (description != null && description != "" && description.length > 0) {
        return `<button class="mw-button" data-bs-toggle="popover" data-bs-custom-class="mw-tooltip" data-bs-trigger='hover' data-bs-content="${escapeHtml(description)}">View Description</button><span class='d-none'>${description}</span>`
    }
    return ""
}

function mw_collapse_label(text = "") {
    return `<div class='d-none d-xl-inline'>${text}</div>`;
}

function mw_id_format(id) {
    return `<i>${id}</i>`
}

function mw_yn_format(bool) {
    if (bool) {
        return "Yes";
    }
    return "No";
}

function mw_list_format(list) {
    if (list.length > 0) {
        var long = `<ul class="d-none d-xl-block ps-0" style='list-style-type: none;'>`
        var short = `<select class='d-inline d-xl-none form-control form-control-sm mw-input' style='width:fit-content;'>`

        for (var i in list) {
            var item = list[i]
            long += `<li>${item}</li>`
            short += `<option>${item}</option>`
        }

        long += `</ul>`
        short += `</select>`;
        return long + short;
    }
    return ""
}

function mw_attribute_list(attributes=[]) {
    var div = `<div class='text-nowrap'  style='line-height:32px;'>`;
    for (var a in attributes) {
        div += `<div class="d-inline d-xl-block">${mw_attribute_icon(attributes[a])} <div class="d-none d-xl-inline">${mw_titleCase(attributes[a])}</div></div>`
    }
    div += "</div>";
    return div;
}

function mw_spell_list(spells=[], collapse=true) {
    var div = `<div class='text-nowrap' style='line-height:32px;'>`;
    for (var s in spells) {
        var spell = spells[s]
        var icon = mw_spellIcon_lookup[spell];
        var name = mw_spell_lookup[spell];
        div += `<div ${collapse ? 'class="d-inline d-xl-block"' : ""}>${mw_magicEffect_icon(icon, mw_titleCase(name))} <div class="${collapse ? 'd-none d-xl-inline' : "d-inline"}">${mw_titleCase(name)}</div></div>`
    }
    div += "</div>";
    return div;
}

function mw_skill_list(skills = []) {
    var div = `<div  class='text-nowrap' style='line-height:32px;'>`;
    for (var a in skills) {
        div += `<div class="d-inline d-xl-block">${mw_skill_icon(skills[a])} <div class="d-none d-xl-inline">${mw_titleCase(mw_gmst_lookup["sskill" + skills[a].toLowerCase()])}</div></div>`
    }
    div += "</div>";
    return div;
}

function mw_titleCase(str) {
    if (str != null && str != undefined) {
    str = str.toLowerCase().split(' ');
    for (var i = 0; i < str.length; i++) {
        str[i] = str[i].charAt(0).toUpperCase() + str[i].slice(1);
    }
    return str.join(' ');
    }
    return "";
}

function mw_textFilter(ix, tbl, w) {
    return `<input type='text' data-ix='${ix}' data-table='${tbl}' class='mw-table-filter' placeholder='Filter' style='width:${(w * 10)}em;' />`;
}

function mw_selectFilter(ix, opts) {
    var sel = `<select data-ix='${id}'>`;
    for (i in opts) {
        var o = opts[i]
        sel += `<option value='${o}'>${o}</option>`
    }
    return sel + "</select>";
}

//#endregion

//#region Table Generators
var mw_gmst_init = false;
function mw_gmst_browser(container) {
    mw_showPanel(container);
    if (!mw_gmst_init) {
        $.get({
            url: "json/morroweb.gmst.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Name" },
                    { title: "Value" }
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([key, data[key]]);
                }
                mw_newDataTable(container, "gmst", columns, rows);
                mw_gmst_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_global_init = false;
function mw_global_browser(container) {
    mw_showPanel(container);
    if (!mw_global_init) {
        $.get({
            url: "json/morroweb.global.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Name" },
                    { title: "Value" }
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([key, data[key]]);
                }
                mw_newDataTable(container, "global", columns, rows);
                mw_global_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_skill_init = false;
function mw_skill_browser(container) {
    mw_showPanel(container);
    if (!mw_skill_init) {
        $.get({
            url: "json/morroweb.skill.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    {   title: "Name", 
                        render: (i) => `${mw_skill_icon(i.toLowerCase())} ${mw_gmst_lookup["sskill" + i.toLowerCase()]}` 
                    },
                    {   title: "Attribute",
                        render: (a) => `${mw_attribute_icon(mw_attributeMap[a])} ${mw_titleCase(mw_attributeMap[a])}`
                    },
                    { title: "Specialisation", render: (s) => mw_titleCase(mw_specialisationMap[s]) },
                    { title: "Description", render: mw_description },
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([
                        key, 
                        key,
                        data[key]["Attribute"],
                        data[key]["Specialisation"],
                        data[key]["Description"],
                    ]);
                }
                mw_newDataTable(container, "skill", columns, rows);
                mw_skill_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_class_init = false;
function mw_class_browser(container) {
    mw_showPanel(container);
    if (!mw_class_init) {
        $.get({
            url: "json/morroweb.class.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    { title: "Name" },
                    { title: "Specialisation" },
                    { title: "Attributes", render: (s) => mw_attribute_list(s) },
                    { title: "Major Skills", render: (s) => mw_skill_list(s) },
                    { title: "Minor Skills", render: (s) => mw_skill_list(s) },
                    { title: "Playable", render: mw_yn_format },
                    { title: "Services", render: (s) => mw_list_format(s.map((i) => mw_titleCase(i.replace("_", " ").replace("_", " ")))) },
                    { title: "Description", render: mw_description },
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([
                        key,
                        data[key]["Name"],
                        data[key]["Specialisation"],
                        data[key]["Attributes"],
                        data[key]["Major"],
                        data[key]["Minor"],
                        data[key]["Playable"],
                        data[key]["Services"],
                        data[key]["Description"],
                    ]);
                }
                var filters = [1,1,1,1,1,1,.5,1,1]
                mw_newDataTable(container, "class", columns, rows, filters);
                mw_class_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_magiceffect_init = false;
function mw_magicEffect_browser(container) {
    mw_showPanel(container);
    if (!mw_magiceffect_init) {
        $.get({
            url: "json/morroweb.magiceffect.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    {
                        title: "Name", 
                        class: "text-nowrap", 
                        render: function (e) {
                        var nm = mw_gmst_lookup[`seffect${e.toLowerCase()}`]
                        if (nm == null || nm == undefined || nm == "") {
                            nm = e
                        }
                        return `${mw_magicEffect_icon(data[e]["Icon"], nm)} ${nm}`
                    } },
                    { title: "School", render: (a) => `${mw_skill_icon(a)} ${a}` },
                    { title: "Cost" },
                    { title: "Speed" },
                    { title: "Size" },
                    { title: "Size Cap" },
                    { title: "Spellmaking", render: mw_yn_format },
                    { title: "Enchanting", render: mw_yn_format },
                    { title: "Description", render: mw_description },
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([
                        key,
                        key,
                        data[key]["School"],
                        data[key]["Cost"],
                        data[key]["Speed"],
                        data[key]["Size"],
                        data[key]["SizeCap"],
                        data[key]["Spellmaking"],
                        data[key]["Enchanting"],
                        data[key]["Description"],
                    ]);
                }
                var filters = [1, 1, 1, .5, .5, .5, .5, .5, .5, 1]
                mw_newDataTable(container, "magiceffect", columns, rows, filters);
                mw_magiceffect_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_spell_init = false;
function mw_spell_browser(container) {
    mw_showPanel(container);
    if (!mw_spell_init) {
        $.get({
            url: "json/morroweb.spell.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    { title: "Name", class: "text-nowrap" },
                    { title: "Type" },
                    { title: "Cost" },
                    { title: "Effect", class: "text-nowrap", render: function (e) {
                        var r = "";
                        for (var ef in e) {
                            var eff = e[ef]["Effect"]
                            var nm = mw_gmst_lookup[`seffect${eff.toLowerCase()}`]
                            if (nm == null || nm == undefined || nm == "") {
                                nm = eff
                            }
                            r += `<div>${mw_magicEffect_icon(mw_effectIcon_lookup[eff], nm)} ${nm}</div>`;
                        }
                        return r;
                    } },
                    { title: "Target", class: "text-nowrap", render: function (e) {
                        var r = "";
                        for (var ef in e) {
                            var at = e[ef]["Attribute"]
                            var sk = e[ef]["Skill"]
                            var t = ""
                            if (at != "None") {
                                r += `<div style='height:2rem'>${mw_attribute_icon(at)} ${at}</div>`;
                            }
                            else if (sk != "None") {
                                r += `<div style='height:2rem'>${mw_skill_icon(sk)} ${mw_gmst_lookup["sskill" + sk.toLowerCase()]}</div>`;
                            }
                            else {
                                r += "<div style='height:2rem'>&nbsp;</div>"
                            }
                        }
                        return r;
                    } },
                    { title: "Range", render: (r) => r.map((e) => `<div style='height:2rem'>${e["Range"].replace("On", "")}</div>`).join("") },
                    { title: "Area", render: (r) => r.map((e) => `<div style='height:2rem'>${e["Area"]}</div>`).join("") },
                    { title: "Duration", render: (r) => r.map((e) => `<div style='height:2rem'>${e["Duration"]}</div>`).join("") },
                    { title: "Magnitude", render: (r) => r.map((e) => `<div style='height:2rem'>${e["Magnitude"][0]} - ${e["Magnitude"][1]}</div>`).join("") }
                ];
                var rows = [];
                for (var key in data) {
                    var itm = data[key]
                    rows.push([
                        key,
                        itm["Name"],
                        itm["Type"],
                        itm["Cost"],
                        itm["Effects"],
                        itm["Effects"],
                        itm["Effects"],
                        itm["Effects"],
                        itm["Effects"],
                        itm["Effects"]
                    ]);
                }
                var filters = [1,1,.5,.5,1,1,.5,.5,.5,.5]
                mw_newDataTable(container, "spell", columns, rows, filters);
                mw_spell_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_race_init = false;
function mw_race_browser(container) {
    mw_showPanel(container);
    if (!mw_race_init) {
        $.get({
            url: "json/morroweb.race.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    { title: "Name" },
                    { title: "Spells", class: "text-nowrap", render: (s) => mw_spell_list(s, false) },
                    { title: "Attributes", render: function (attr) {
                        var div = `<div style='line-height:32px;'>`;
                        for (var a in attr) {
                            div += `<div class='row'><div class='col-3 text-nowrap'>${attr[a][0]} / ${attr[a][1]}</div><div class='col'>${mw_attribute_icon(a)} ${mw_titleCase(a)}</div></div>`
                        }
                        div += "</div>";
                        return div;
                    } },
                    { title: "Skills", render: function (skill) {
                        var div = `<div style='line-height:32px;'>`;
                        for (var s in skill) {
                            div += `<div class='row'><div class='col-2 text-nowrap'>+${skill[s]}</div><div class='col'>${mw_skill_icon(s)} ${mw_titleCase(mw_gmst_lookup["sskill" + s.toLowerCase()])}</div></div>`
                        }
                        div += "</div>";
                        return div;
                    }
                    },
                    { title: "Height", render: (h) => `${h[0]} / ${h[1]}` },
                    { title: "Weight", render: (h) => `${h[0]} / ${h[1]}` },
                    { title: "Playable", render: mw_yn_format },
                    { title: "Beast", render: mw_yn_format },
                    { title: "Description", render: mw_description },
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([
                        key,
                        data[key]["Name"],
                        data[key]["Spells"],
                        data[key]["Attributes"],
                        data[key]["SkillBonuses"],
                        data[key]["Height"],
                        data[key]["Weight"],
                        data[key]["Playable"],
                        data[key]["Beast"],
                        data[key]["Description"],
                    ]);
                }
                var filters = [1, 1, 1, 1, 1, .5, .5, .5, .5,1]
                mw_newDataTable(container, "race", columns, rows, filters);
                mw_race_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_faction_init = false;
function mw_faction_browser(container) {
    mw_showPanel(container);
    if (!mw_faction_init) {
        $.get({
            url: "json/morroweb.faction.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    { title: "Name" },
                    { title: "Ranks", render: (r) => mw_list_format(r.map((v, i) => `${i} ${v}`))},
                    { title: "Attributes", render: mw_attribute_list },
                    { title: "Skills", render: mw_skill_list },
                    { title: "Reactions", render: (r) => {
                        var list = []
                        for (var k in r) {
                            list.push(`${r[k] > 0 ? "+" : ""}${r[k]} ${k}`)
                        }
                        return mw_list_format(list);
                    }}
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([
                        key,
                        data[key]["Name"],
                        data[key]["Ranks"],
                        data[key]["Attributes"],
                        data[key]["Skills"],
                        data[key]["Reactions"],
                    ]);
                }
                var filters = [1, 1, 1, 1, 1, 1]
                mw_newDataTable(container, "faction", columns, rows, filters);
                mw_faction_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_miscitem_init = false;
function mw_miscItem_browser(container) {
    mw_showPanel(container);
    if (!mw_miscitem_init) {
        $.get({
            url: "json/morroweb.miscitem.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    { title: "Name", class: "text-nowrap", render: (k) => `${mw_item_icon(data[k]["Icon"], data[k]["Name"])} ${data[k]["Name"]}` },
                    { title: "Weight" },
                    { title: "Value" },
                    { title: "Key", render: (k) => mw_yn_format(k == "KEY") }
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([
                        key,
                        key,
                        data[key]["Weight"],
                        data[key]["Value"],
                        data[key]["Flags"]
                    ]);
                }
                var filters = [1, 1, .5, .5, .5]
                mw_newDataTable(container, "miscitem", columns, rows, filters);
                mw_miscitem_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_lockpick_init = false;
function mw_lockpick_browser(container) {
    mw_showPanel(container);
    if (!mw_lockpick_init) {
        $.get({
            url: "json/morroweb.lockpick.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    { title: "Name", class: "text-nowrap", render: (k) => `${mw_item_icon(data[k]["Icon"], data[k]["Name"])} ${data[k]["Name"]}` },
                    { title: "Weight" },
                    { title: "Value" },
                    { title: "Quality" },
                    { title: "Uses" }
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([
                        key,
                        key,
                        data[key]["Weight"],
                        data[key]["Value"],
                        data[key]["Quality"],
                        data[key]["Uses"]
                    ]);
                }
                var filters = [1, 1, .5, .5, .5, .5]
                mw_newDataTable(container, "lockpick", columns, rows, filters);
                mw_lockpick_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_repairitem_init = false;
function mw_repairItem_browser(container) {
    mw_showPanel(container);
    if (!mw_repairitem_init) {
        $.get({
            url: "json/morroweb.repairitem.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    { title: "Name", class: "text-nowrap", render: (k) => `${mw_item_icon(data[k]["Icon"], data[k]["Name"])} ${data[k]["Name"]}` },
                    { title: "Weight" },
                    { title: "Value" },
                    { title: "Quality" },
                    { title: "Uses" }
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([
                        key,
                        key,
                        data[key]["Weight"],
                        data[key]["Value"],
                        data[key]["Quality"],
                        data[key]["Uses"]
                    ]);
                }
                var filters = [1, 1, .5, .5, .5, .5]
                mw_newDataTable(container, "repairitem", columns, rows, filters);
                mw_repairitem_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_probe_init = false;
function mw_probe_browser(container) {
    mw_showPanel(container);
    if (!mw_probe_init) {
        $.get({
            url: "json/morroweb.probe.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    { title: "Name", class: "text-nowrap", render: (k) => `${mw_item_icon(data[k]["Icon"], data[k]["Name"])} ${data[k]["Name"]}` },
                    { title: "Weight" },
                    { title: "Value" },
                    { title: "Quality" },
                    { title: "Uses" }
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([
                        key,
                        key,
                        data[key]["Weight"],
                        data[key]["Value"],
                        data[key]["Quality"],
                        data[key]["Uses"]
                    ]);
                }
                var filters = [1, 1, .5, .5, .5, .5]
                mw_newDataTable(container, "probe", columns, rows, filters);
                mw_probe_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_apparatus_init = false;
function mw_apparatus_browser(container) {
    mw_showPanel(container);
    if (!mw_apparatus_init) {
        $.get({
            url: "json/morroweb.apparatus.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    { title: "Name", class: "text-nowrap", render: (k) => `${mw_item_icon(data[k]["Icon"], data[k]["Name"])} ${data[k]["Name"]}` },
                    { title: "Type" },
                    { title: "Weight" },
                    { title: "Value" },
                    { title: "Quality" },
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([
                        key,
                        key,
                        data[key]["Type"],
                        data[key]["Weight"],
                        data[key]["Value"],
                        data[key]["Quality"],
                    ]);
                }
                var filters = [1, 1, .5, .5, .5, .5]
                mw_newDataTable(container, "apparatus", columns, rows, filters);
                mw_apparatus_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_alchemy_init = false;
function mw_alchemy_browser(container) {
    mw_showPanel(container);
    if (!mw_alchemy_init) {
        $.get({
            url: "json/morroweb.alchemy.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    { title: "Name", class: "text-nowrap", render: (k) => `${mw_item_icon(data[k]["Icon"], data[k]["Name"])} ${data[k]["Name"]}` },
                    { title: "Weight" },
                    { title: "Value" },
                    {
                        title: "Effect", class: "text-nowrap", render: function (e) {
                            var r = "";
                            for (var ef in e) {
                                var eff = e[ef]["Effect"]
                                var nm = mw_gmst_lookup[`seffect${eff.toLowerCase()}`]
                                if (nm == null || nm == undefined || nm == "") {
                                    nm = eff
                                }
                                r += `<div>${mw_magicEffect_icon(mw_effectIcon_lookup[eff], nm)} ${nm}</div>`;
                            }
                            return r;
                        }
                    },
                    {
                        title: "Target", class: "text-nowrap", render: function (e) {
                            var r = "";
                            for (var ef in e) {
                                var at = e[ef]["Attribute"]
                                var sk = e[ef]["Skill"]
                                var t = ""
                                if (at != "None") {
                                    r += `<div style='height:2rem'>${mw_attribute_icon(at)} ${at}</div>`;
                                }
                                else if (sk != "None") {
                                    r += `<div style='height:2rem'>${mw_skill_icon(sk)} ${mw_gmst_lookup["sskill" + sk.toLowerCase()]}</div>`;
                                }
                                else {
                                    r += "<div style='height:2rem'>&nbsp;</div>"
                                }
                            }
                            return r;
                        }
                    },
                    { title: "Range", render: (r) => r.map((e) => `<div style='height:2rem'>${e["Range"].replace("On", "")}</div>`).join("") },
                    { title: "Area", render: (r) => r.map((e) => `<div style='height:2rem'>${e["Area"]}</div>`).join("") },
                    { title: "Duration", render: (r) => r.map((e) => `<div style='height:2rem'>${e["Duration"]}</div>`).join("") },
                    { title: "Magnitude", render: (r) => r.map((e) => `<div style='height:2rem'>${e["Magnitude"][0]} - ${e["Magnitude"][1]}</div>`).join("") }
                ];
                var rows = [];
                for (var key in data) {
                    var itm = data[key]
                    rows.push([
                        key,
                        key,
                        itm["Weight"],
                        itm["Value"],
                        itm["Effects"],
                        itm["Effects"],
                        itm["Effects"],
                        itm["Effects"],
                        itm["Effects"],
                        itm["Effects"]
                    ]);
                }
                var filters = [1, 1, .5, .5, 1, 1, .5, .5, .5, .5]
                mw_newDataTable(container, "alchemy", columns, rows, filters);
                mw_alchemy_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

var mw_birthsign_init = false;
function mw_birthsign_browser(container) {
    mw_showPanel(container);
    if (!mw_birthsign_init) {
        $.get({
            url: "json/morroweb.birthsign.json",
            cache: true,
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    { title: "Name" },
                    { title: "Spells", class: "text-nowrap", render: (s) => mw_spell_list(s, false) },
                    { title: "Description", render: mw_description },
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([
                        key,
                        data[key]["Name"],
                        data[key]["Spells"],
                        data[key]["Description"],
                    ]);
                }
                var filters = [1, 1, 1, 1]
                mw_newDataTable(container, "birthsign", columns, rows, filters);
                mw_birthsign_init = true;
            }
        });
    }
    else {
        currentTable = allTables[container];
        mw_applyFilters(container);
    }
}

//#endregion