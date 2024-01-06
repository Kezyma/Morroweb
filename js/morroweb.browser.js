function mwb_init() {
    $.get({
        url: "json/morroweb.gmst.json",
        success: function (data) {
            mwb_gmst = {}
            for (key in data) {
                mwb_gmst[key.toLowerCase()] = data[key];
            }
        }
    });
    $.get({
        url: "json/morroweb.magiceffect.json",
        success: function (data) {
            mwb_magicEffect = data;
        }
    });
    $.get({
        url: "json/morroweb.skill.json",
        success: function (data) {
            mwb_skill = data;
        }
    });
    $.get({
        url: "json/morroweb.spell.json",
        success: function (data) {
            mwb_spell = data;
        }
    });
}

var mwb_gmst = null;
var mwb_magicEffect = null;
var mwb_skill = null;
var mwb_spell = null;

var mwb_specialisationMap = ["combat", "magic", "stealth"]
var mwb_attributeMap = ["strength", "intelligence", "willpower", "agility", "speed", "endurance", "personality", "luck"];

function mwb_createTable(container, columns, dataSet) {
    var cardBody = $(`#${container}`);
    var newTable = $(`<table id='${container}-datatable' class='mwb-table table-sm table-borderless display nowrap table table-compact table-striped w-100 responsive'></table>`);
    cardBody.append(newTable);
    currentTable = new DataTable(`#${container}-datatable`, {
        columns: columns,
        data: dataSet,
        scrollX: true,
        sScrollX: "100%",
        paging: false,
        scrollCollapse: true,
        scrollY: '60vh'
    });

    var tooltipTriggerList = $(`#${container} [data-bs-toggle="tooltip"]`);
    [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))
}

function mwb_attributeIcon(attribute, showLabel = true, collapse = true, title = false) {
    if (attribute != null && attribute != undefined)
    {
        var label = "";
        if (showLabel) {
            label = "<div class='" + (collapse ? "d-none d-xl-inline" : "d-inline") + "'>" + (title ? "<b>" : "") + attribute + (title ? "</b>" : "") + "</div>"
        }
        return `<img src='img\\icons\\attribute\\attribute_${attribute.toLowerCase()}.png' data-bs-toggle="tooltip" data-bs-title='${attribute}' /> ${label}`;
    }
    return ""
}

function mwb_skillIcon(skill, showLabel = true, collapse = true, title = false) {
    if (skill != "None") {
        var label = "";
        if (showLabel) {
            label = "<div class='" + (collapse ? "d-none d-xl-inline" : "d-inline") + "'>" + (title ? "<b>" : "") + mwb_gmst["sskill" + skill.toLowerCase()] + (title ? "</b>" : "") + "</div>"
        }
        return `<img src='img\\icons\\skill\\${skill.toLowerCase()}.png' data-bs-toggle="tooltip" data-bs-title='${mwb_gmst["sskill" + skill.toLowerCase()]}' /> ${label}`;
    }
}

function mwb_magicEffectIcon(effect, showLabel = true, collapse = true, title = false) {
    var item = mwb_magicEffect[effect];
    var lookup = `seffect${effect.toLowerCase()}`;
    var label = "";
    if (showLabel) {
        label = "<div class='" + (collapse ? "d-none d-xl-inline" : "d-inline") + "'>" + (title ? "<b>" : "") + (mwb_gmst[lookup] ?? effect) + (title ? "</b>" : "") + "</div>"
    }
    var res = `<img src='img\\icons\\magiceffect\\b_${item["Icon"].toLowerCase()}.png' data-bs-toggle="tooltip" data-bs-title='${mwb_gmst[lookup] ?? effect}' /> ${label}`;
    return res;
}

function mwb_itemIcon(name, icon, showLabel = true, collapse = true, title = false) {
    if (name != null && name != undefined && name != "") {
        var label = "";
        if (showLabel) {
            label = "<div class='" + (collapse ? "d-none d-xl-inline" : "d-inline") + "'>" + (title ? "<b>" : "") + name + (title ? "</b>" : "") + "</div>"
        }
        return `<img src='img\\icons\\item\\${icon.toLowerCase()}.png' data-bs-toggle="tooltip" data-bs-title='${name}' /> ${label}`;
    }
    if (showLabel) {
        return `${(title ? "<b>" : "")}${name}${(title ? "</b>" : "")}`
    }
    return ""
}

function mwb_titleCase(str) {
    str = str.toLowerCase().split(' ');
    for (var i = 0; i < str.length; i++) {
        str[i] = str[i].charAt(0).toUpperCase() + str[i].slice(1);
    }
    return str.join(' ');
}

function mwb_col_idFormat(id) {
    return `<i>${id}</i>`;
}

function mwb_col_nameFormat(name) {
    return `<b>${name}</b>`;
}
 

function mwb_col_yesNoFormat(bool) {
    if (bool) {
        return "Yes";
    }
    return "No";
}

function mwb_col_attrListFormat(attr) {
    return attr.map((attrObj) => mwb_attributeIcon(attrObj)).join("<br />");
}

function mwb_col_skillListFormat(skill) {
    return skill.map((attrObj) => mwb_skillIcon(attrObj)).join("<br />");
}

function mwb_col_magicEffectListFormat(effect) {
    return "";
}

function mwb_browserTab() {
    $(".mwb-panel").hide();
    $(`#mwb-home-panel`).show();
    $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
}

var mwb_class_init = false;
function mwb_classTable() {
    if (!mwb_class_init) {
        $.get({
            url: "json/morroweb.class.json",
            success: function (data) {

                var dataSet = [];
                for (var key in data) {
                    var dataObj = data[key];
                    dataSet.push([
                        key, 
                        dataObj["Name"], 
                        dataObj["Specialisation"], 
                        dataObj["Attributes"],
                        dataObj["Major"],
                        dataObj["Minor"],
                        dataObj["Playable"]
                    ]);
                }
                var columns = [
                    { title: "Id", render: mwb_col_idFormat },
                    { title: "Name", render: mwb_col_nameFormat },
                    { title: "Specialisation" },
                    { title: "Attributes", render: mwb_col_attrListFormat, },
                    { title: "Major Skills", render: mwb_col_skillListFormat },
                    { title: "Minor Skills", render: mwb_col_skillListFormat },
                    { title: "Playable", render: mwb_col_yesNoFormat }
                ];
                mwb_createTable("mwb-class-table", columns, dataSet);
                mwb_class_init = true;
            }
        });
    }
    $(".mwb-panel").hide();
    $(`#mwb-class-table`).show();
    $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
}

var mwb_faction_init = false;
function mwb_factionTable() {
    if (!mwb_faction_init) {
        $.get({
            url: "json/morroweb.faction.json",
            success: function (data) {
                var dataSet = [];
                for (var key in data) {
                    var dataObj = data[key];
                    dataSet.push([
                        key,
                        dataObj["Name"],
                        dataObj["Ranks"],
                        dataObj["Attributes"],
                        dataObj["Skills"]
                    ]);
                }
                var columns = [
                    { title: "Id", render: mwb_col_idFormat },
                    { title: "Name", render: mwb_col_nameFormat },
                    { 
                        title: "Ranks",
                        render: function(data) {
                            var list = "<ol start='0' style='line-height: 20px;'>";
                            if (data.length > 0) {
                                for (rank in data) {
                                    var itm = `<li>${data[rank]}</li>`;
                                    list += itm;
                                }
                            }
                            list += "</ol>";
                            return list;
                        }
                    },
                    { title: "Attributes", render: mwb_col_attrListFormat, },
                    { title: "Skills", render: mwb_col_skillListFormat, }
                ];
                mwb_createTable("mwb-faction-table", columns, dataSet);
                mwb_faction_init = true;
            }
        });
    }
    $(".mwb-panel").hide();
    $(`#mwb-faction-table`).show();
    $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
}

var mwb_magiceffect_init = false;
function mwb_magicEffectTable() {
    if (!mwb_magiceffect_init) {
        var data = mwb_magicEffect;
        var dataSet = [];
        for (var key in data) {
            var dataObj = data[key];
            dataSet.push([
                key,
                key,
                dataObj["School"],
                dataObj["BaseCost"],
                dataObj["Speed"],
                dataObj["Size"],
                dataObj["SizeCap"],
                dataObj["Spellmaking"],
                dataObj["Enchanting"]
            ]);
        }
        console.log(dataSet);
        var columns = [
            { title: "Id", render: mwb_col_idFormat },
            { title: "Name", render: (val) => mwb_magicEffectIcon(val, true, false, true) },
            { title: "School", render: mwb_skillIcon },
            { title: "Cost" },
            { title: "Speed" },
            { title: "Size" },
            { title: "Size Cap" },
            { title: "Spellmaking", render: mwb_col_yesNoFormat },
            { title: "Enchanting", render: mwb_col_yesNoFormat },
        ];
        mwb_createTable("mwb-magiceffect-table", columns, dataSet);
        mwb_magiceffect_init = true;
    }
    $(".mwb-panel").hide();
    $(`#mwb-magiceffect-table`).show();
    $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
}

var mwb_skill_init = false;
function mwb_skillTable() {
    if (!mwb_skill_init) {
        $.get({
            url: "json/morroweb.skill.json",
            success: function (data) {
                var dataSet = [];
                for (var key in data) {
                    var dataObj = data[key];
                    dataSet.push([
                        key,
                        key,
                        dataObj["Attribute"],
                        dataObj["Specialisation"]
                    ]);
                }
                console.log(dataSet);
                var columns = [
                    { title: "Id", render: mwb_col_idFormat },
                    { title: "Name", render: (val) => mwb_skillIcon(val, true, false, true) },
                    { title: "Attribute", render: (val) => mwb_attributeIcon(mwb_titleCase(mwb_attributeMap[val])) },
                    { title: "Specialisation", render: (val) => mwb_titleCase(mwb_specialisationMap[val]) }
                ];
                mwb_createTable("mwb-skill-table", columns, dataSet);
                mwb_skill_init = true;
            }
        });
    }
    $(".mwb-panel").hide();
    $(`#mwb-skill-table`).show();
    $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
}

var mwb_spell_init = false;
function mwb_spellTable() {
    if (!mwb_spell_init) {
        var data = mwb_spell
        var dataSet = [];
        for (var key in data) {
            var dataObj = data[key];
            dataSet.push([
                key,
                dataObj["Name"],
                dataObj["Cost"],
                dataObj["Effects"],
                dataObj["Effects"],
                dataObj["Effects"],
                dataObj["Effects"],
                dataObj["Effects"]
            ]);
        }
        console.log(dataSet);
        var columns = [
            { title: "Id", render: mwb_col_idFormat },
            { title: "Name", render: mwb_col_nameFormat },
            { title: "Cost" },
            { title: "Range", render: (effects) => effects[0]["Range"].replace("On", "") },
            { title: "Effects", render: function (effects) {
                    var res = "";
                    for (var ix in effects) {
                        var effect = effects[ix];
                        var html = mwb_magicEffectIcon(effect["Effect"])

                        if (effect["Skill"] != "None") {
                            html += " " + mwb_skillIcon(effect["Skill"])
                        }
                        if (effect["Attribute"] != "None") {
                            html += " " + mwb_attributeIcon(effect["Attribute"])
                        }
                        if (res.length == 0) {
                            res = html;
                        }
                        else {
                            res += `<br />${html}`;
                        }
                    }
                    return res;
                } 
            },
            {
                title: "Magnitude", render: function (effects) {
                    var res = "";
                    for (var ix in effects) {
                        var html = "<span>"
                        var effect = effects[ix];
                        var minMag = effect["Magnitude"][0]
                        var maxMag = effect["Magnitude"][1]
                        if (minMag == maxMag) {
                            html += `${minMag}`
                        }
                        else {
                            html += `${minMag}-${maxMag}`
                        }

                        if (res.length == 0) {
                            res = html;
                        }
                        else {
                            res += `<br />${html}`;
                        }
                    }
                    return res + "</span>";
                }
            },
            {
                title: "Duration", render: function (effects) {
                    var res = "";
                    for (var ix in effects) {
                        var effect = effects[ix];
                        var html = `<span>${effect["Duration"]}</span>`
                        if (res.length == 0) {
                            res = html;
                        }
                        else {
                            res += `<br />${html}`;
                        }
                    }
                    return res;
                }
            },
            {
                title: "Area", render: function (effects) {
                    var res = "";
                    for (var ix in effects) {
                        var effect = effects[ix];
                        var html = `<span style='line-height:32px;'>${effect["Area"]}</span>`
                        if (res.length == 0) {
                            res = html;
                        }
                        else {
                            res += `<br />${html}`;
                        }
                    }
                    return res;
                }
            }
        ];
        mwb_createTable("mwb-spell-table", columns, dataSet);
        mwb_spell_init = true;
    }
    $(".mwb-panel").hide();
    $(`#mwb-spell-table`).show();
    $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
}

var mwb_race_init = false;
function mwb_raceTable() {
    if (!mwb_race_init) {
        $.get({
            url: "json/morroweb.race.json",
            success: function (data) {
                var dataSet = [];
                for (var key in data) {
                    var dataObj = data[key];
                    dataSet.push([
                        key,
                        dataObj["Name"],
                        dataObj["Height"],
                        dataObj["Weight"],
                        dataObj["SkillBonuses"],
                        dataObj["Attributes"],
                        dataObj["Spells"],
                        dataObj["Playable"],
                        dataObj["Beast"]
                    ]);
                }
                console.log(dataSet);
                var columns = [
                    { title: "Id", render: mwb_col_idFormat },
                    { title: "Name", render: mwb_col_nameFormat },
                    { title: "Height", render: (vals) => vals.join("/") },
                    { title: "Weight", render: (vals) => vals.join("/") },
                    { title: "Skill Bonuses", render: function (val) {
                            var res = "";
                            for (var skill in val) {
                                res += "<div class='row row-cols-2'><div class='col' style='max-width:30px;'>" + val[skill] + "</div><div class='col'> " + mwb_skillIcon(skill) + "</div></div>";
                            }
                            return res + "";
                        }
                    },
                    {
                        title: "Attributes", render: function (val) {
                            var res = "";
                            for (var attr in val) {
                                res += "<div class='row row-cols-2'><div class='col' style='max-width:60px;'>" + val[attr].join("/") + "</div><div class='col'> " + mwb_attributeIcon(mwb_titleCase(attr)) + "</div></div>";
                            }
                            return res + "";
                        }
                    },
                    {
                        title: "Spells", render: function (val) {
                            var res = "";
                            for (var sp in val) {
                                var spellItem = mwb_spell[val[sp]]
                                res += `${mwb_magicEffectIcon(spellItem["Effects"][0]["Effect"], false)} ${spellItem["Name"]}<br/>`
                                console.log(spellItem)
                            }
                            return res;
                        }
                    },
                    { title: "Playable", render: mwb_col_yesNoFormat },
                    { title: "Beast", render: mwb_col_yesNoFormat }
                ];
                mwb_createTable("mwb-race-table", columns, dataSet);
                mwb_race_init = true;
            }
        });
    }
    $(".mwb-panel").hide();
    $(`#mwb-race-table`).show();
    $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
}

var mwb_miscitem_init = false;
var mwb_miscItem = null;
function mwb_miscItemTable() {
    if (!mwb_miscitem_init) {
        $.get({
            url: "json/morroweb.miscitem.json",
            success: function (data) {
                var dataSet = [];
                mwb_miscItem = data;
                for (var key in data) {
                    if (key != null && key != undefined && key != "") {
                        var dataObj = data[key];
                        dataSet.push([
                            key,
                            key,
                            dataObj["Weight"],
                            dataObj["Value"]
                        ]);
                    }
                }
                console.log(dataSet);
                var columns = [
                    { title: "Id", render: mwb_col_idFormat },
                    {
                        title: "Name", render: function (val) {
                            var obj = mwb_miscItem[val]
                            return mwb_itemIcon(obj["Name"], obj["Icon"], true, false, true)
                        } },
                    { title: "Weight" },
                    { title: "Value" }
                ];
                mwb_createTable("mwb-miscitem-table", columns, dataSet);
                mwb_miscitem_init = true;
            }
        });
    }
    $(".mwb-panel").hide();
    $(`#mwb-miscitem-table`).show();
    $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
}