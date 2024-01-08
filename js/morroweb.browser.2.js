
//#region Constants
const mw_specialisationMap = ["combat", "magic", "stealth"]
const mw_attributeMap = ["strength", "intelligence", "willpower", "agility", "speed", "endurance", "personality", "luck"];
//#endregion

//#region Pre-Load
function mw_preLoad() {
    $.get({
        url: "json/morroweb.gmst.json",
        success: function (data) {
            for (var s in data) {
                mw_gmst_lookup[s.toLowerCase()] = data[s]
            }
        }
    });
}
var mw_gmst_lookup = {}

$(document).ready(function () {
    mw_preLoad();
    //setInterval(mw_resize, 1000);
});

function mw_resize() {
    $(window).trigger("resize");
}
//#endregion

//#region Table Functions
function mw_newTable(id) {
    return $(`<table id='mw_${id}_table' class='mw-table table-sm table-borderless display table table-compact table-striped'></table>`);
}

function mw_newDataTable(container, id, headings, rows) {
    // Generate the DataTable.
    var table = mw_newTable(id);
    var cardBody = $(`#${container}`);
    cardBody.append(table);
    currentTable = new DataTable(`#mw_${id}_table`, {
        columns: headings,
        data: rows,
        paging: false,
        scrollCollapse: true,
        scrollY: true,
        scrollY: "calc(100vh - 18rem)",
        scrollX: true,
        sScrollX: "100%",
        initComplete: function (settings) {

            var ft = $("<tr></tr>");
            for (var i in headings) {
                ft.append($(`<th>${mw_textFilter(i)}</th>`))
            }
            $(settings.nTHead).append(ft);

            mw_resize();
            setTimeout(mw_resize, 1000);
        }
    });

    $(currentTable.table().container()).on('keyup', 'thead input', function () {
        currentTable
            .column($(this).data('ix'))
            .search(this.value)
            .draw();
    });

    // Initialise Bootstrap tooltips.
    var tooltipTriggerList = $(`#mw_${id}_table [data-bs-toggle="tooltip"]`);
    [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))

    
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

function mw_magicEffect_icon(icon = "", title = "") {
    if (icon != "" && icon != "none") {
        return mw_icon(`magiceffect/b_${icon.toLowerCase()}.png`, title)
    }
    return "";
}

function mw_icon(url, title) {
    return `<img src='img/icons/${url}' data-bs-toggle="tooltip" data-bs-custom-class="mw-tooltip" data-bs-title='${title}' />`
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
        var long = `<ul class="d-none d-xl-block">`
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
    var div = `<div>`;
    for (var a in attributes) {
        div += `<div class="d-inline d-xl-block">${mw_attribute_icon(attributes[a])} <div class="d-none d-xl-inline">${mw_titleCase(attributes[a])}</div></div>`
    }
    div += "</div>";
    return div;
}

function mw_skill_list(skills = []) {
    var div = `<div>`;
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

function mw_textFilter(ix) {
    return `<input type='text' data-ix='${ix}' class='' placeholder='Filter' />`;
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
}

var mw_global_init = false;
function mw_global_browser(container) {
    mw_showPanel(container);
    if (!mw_global_init) {
        $.get({
            url: "json/morroweb.global.json",
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
}

var mw_skill_init = false;
function mw_skill_browser(container) {
    mw_showPanel(container);
    if (!mw_skill_init) {
        $.get({
            url: "json/morroweb.skill.json",
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    {   title: "Name", 
                        render: (i) => `${mw_skill_icon(i.toLowerCase())} ${mw_gmst_lookup["sskill" + i.toLowerCase()]}` 
                    },
                    {   title: "Attribute",
                        render: (a) => `${mw_attribute_icon(mw_attributeMap[a])} ${mw_titleCase(mw_attributeMap[a])}`
                    },
                    { title: "Specialisation", render: (s) => mw_titleCase(mw_specialisationMap[s]) }
                ];
                var rows = [];
                for (var key in data) {
                    rows.push([
                        key, 
                        key,
                        data[key]["Attribute"],
                        data[key]["Specialisation"],
                    ]);
                }
                mw_newDataTable(container, "skill", columns, rows);
                mw_skill_init = true;
            }
        });
    }
}

var mw_class_init = false;
function mw_class_browser(container) {
    mw_showPanel(container);
    if (!mw_class_init) {
        $.get({
            url: "json/morroweb.class.json",
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    { title: "Name" },
                    { title: "Specialisation" },
                    { title: "Attributes", render: (s) => mw_attribute_list(s) },
                    { title: "Major Skills", render: (s) => mw_skill_list(s) },
                    { title: "Minor Skills", render: (s) => mw_skill_list(s) },
                    { title: "Playable", render: mw_yn_format },
                    { title: "Services", render: (s) => mw_list_format(s.map((i) => mw_titleCase(i.replace("_", " ").replace("_", " ")))) }
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
                        data[key]["Services"]
                    ]);
                }
                mw_newDataTable(container, "class", columns, rows);
                mw_class_init = true;
            }
        });
    }
}

var mwb_magiceffect_init = false;
function mw_magicEffect_browser(container) {
    mw_showPanel(container);
    if (!mwb_magiceffect_init) {
        $.get({
            url: "json/morroweb.magiceffect.json",
            success: function (data) {
                var columns = [
                    { title: "Id", render: mw_id_format },
                    {
                        title: "Name", 
                        class: "text-nowrap", 
                        render: function (e) {
                        var nm = mw_gmst_lookup[`seffect${e.toLowerCase()}`]
                        if (nm == null || nm == undefined || nm == "") {
                            nm = key
                        }
                        return `${mw_magicEffect_icon(data[e]["Icon"], nm)} ${nm}`
                    } },
                    { title: "School", render: (a) => `${mw_skill_icon(a)} ${a}` },
                    { title: "Cost" },
                    { title: "Speed" },
                    { title: "Size" },
                    { title: "Size Cap" },
                    { title: "Spellmaking", render: mw_yn_format },
                    { title: "Enchanting", render: mw_yn_format }
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
                        data[key]["Enchanting"]
                    ]);
                }
                mw_newDataTable(container, "magiceffect", columns, rows);
                mwb_magiceffect_init = true;
            }
        });
    }
}
//#endregion