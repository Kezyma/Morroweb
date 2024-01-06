function mwb_init() {
    $.get({
        url: "json/morroweb.gmst.json",
        success: function (data) {
            mwb_gmst = data;
        }
    });
}

var mwb_gmst = null;

var mwb_specialisationMap = ["combat", "magic", "stealth"]
var mwb_attributeMap = ["strength", "intelligence", "willpower", "agility", "speed", "endurance", "personality", "luck"];

function mwb_createTable(container, columns, dataSet) {
    var cardBody = $(`#${container}`);
    var newTable = $(`<table id='${container}-datatable' class='display nowrap table table-compact table-striped w-100 responsive'></table>`);
    cardBody.append(newTable);
    currentTable = new DataTable(`#${container}-datatable`, {
        columns: columns,
        data: dataSet,
        fixedColumns: true,
        scrollX: true,
        paging: false,
        scrollCollapse: true,
        scrollY: '50vh'
    });

    var tooltipTriggerList = $(`#${container} [data-bs-toggle="tooltip"]`);
    [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))
}

function mwb_attributeIcon(attribute, name) {
    if (attribute != null && attribute != undefined)
    {
        return `<img src='img\\icons\\attribute_${attribute.toLowerCase()}.png' data-bs-toggle="tooltip" data-bs-title='${name ?? attribute}' /><span class='d-none'>${name ?? attribute}</span>`;
    }
    return ""
}

function mwb_skillIcon(skill, name) {
    return `<img src='img\\icons\\${skill.toLowerCase()}.png' data-bs-toggle="tooltip" data-bs-title='${name ?? skill}' /><span class='d-none'>${name ?? skill}</span>`;
}

function mwb_titleCase(str) {
    str = str.toLowerCase().split(' ');
    for (var i = 0; i < str.length; i++) {
        str[i] = str[i].charAt(0).toUpperCase() + str[i].slice(1);
    }
    return str.join(' ');
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
                    {
                        title: "Id",
                        render: (id) => `<i>${id}</i>` },
                    { title: "Name" },
                    { title: "Specialisation" },
                    { 
                        title: "Attributes",
                        render: (data) => data.map((attrObj) => mwb_attributeIcon(attrObj)).join(""),
                    },
                    { 
                        title: "Major Skills",
                        render: (data) => data.map((attrObj) => mwb_skillIcon(attrObj)).join(""),
                    },
                    {
                        title: "Minor Skills",
                        render: (data) => data.map((attrObj) => mwb_skillIcon(attrObj)).join(""),
                    },
                    { 
                        title: "Playable",
                        render: (data) => data == true ? "Yes" : "No"
                    }
                ];
                mwb_createTable("mwb-class-table", columns, dataSet);
                mwb_class_init = true;
            }
        });
    }
    $(".mwb-panel").hide();
    $(`#mwb-class-table`).show();
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
                    {
                        title: "Id",
                        render: (id) => `<i>${id}</i>`
                     },
                    { title: "Name" },
                    { 
                        title: "Ranks",
                        render: function(data) {
                            if (data.length > 0) {
                                var itm = "<select class='form-control form-control-sm'>";
                                for (var rank in data) {
                                    itm += `<option>${rank}: ${data[rank]}</option>`;
                                }
                                itm += "</select>";
                                return itm;
                            }
                            return "";
                        }
                    },
                    {
                        title: "Attributes",
                        render: (data) => data.map((attrObj) => mwb_attributeIcon(attrObj)).join(""),
                    },
                    {
                        title: "Skills",
                        render: (data) => data.map((attrObj) => (attrObj != "None") ? mwb_skillIcon(attrObj, mwb_gmst[`sSkill${mwb_titleCase(attrObj)}`]) : "").join(""),
                    }
                ];
                mwb_createTable("mwb-faction-table", columns, dataSet);
                mwb_faction_init = true;
            }
        });
    }
    $(".mwb-panel").hide();
    $(`#mwb-faction-table`).show();
}

var mwb_magiceffect_init = false;
function mwb_magicEffectTable() {
    if (!mwb_magiceffect_init) {
        $.get({
            url: "json/morroweb.magiceffect.json",
            success: function (data) {
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
                    { 
                        title: "Id",
                        render: (id) => `<i>${id}</i>`
                    },
                    {
                        title: "Name",
                        render: function (val) {
                            var item = data[val];
                            var lookup = `sEffect${val}`;
                            var res = `<img src='img\\icons\\b_${item["Icon"]}.png' data-bs-toggle="tooltip" data-bs-title='${mwb_gmst[lookup] ?? val}' /> ${mwb_gmst[lookup] ?? val}`;
                            return res;
                        }
                    },
                    { 
                        title: "School",
                        render: (val) => `${mwb_skillIcon(val)} ${val}`
                    },
                    { title: "Cost" },
                    { title: "Speed" },
                    { title: "Size" },
                    { title: "Size Cap" },
                    {
                        title: "Spellmaking",
                        render: (data) => data == true ? "Yes" : "No" },
                    {
                        title: "Enchanting",
                        render: (data) => data == true ? "Yes" : "No" },
                ];
                mwb_createTable("mwb-magiceffect-table", columns, dataSet);
                mwb_magiceffect_init = true;
            }
        });
    }
    $(".mwb-panel").hide();
    $(`#mwb-magiceffect-table`).show();
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
                    {
                        title: "Id",
                        render: (id) => `<i>${id}</i>`
                    },
                    {
                        title: "Name",
                        render: function (val) {
                            var lookup = `sSkill${mwb_titleCase(val)}`;
                            return `<img src='img\\icons\\${val}.png' data-bs-toggle="tooltip" data-bs-title='${mwb_gmst[lookup]}' /> ${mwb_gmst[lookup]}`;
                        }
                    },
                    {
                        title: "Attribute",
                        render: (val) => `${mwb_attributeIcon(mwb_attributeMap[val], mwb_titleCase(mwb_attributeMap[val]))} ${mwb_titleCase(mwb_attributeMap[val])}`
                    },
                    { 
                        title: "Specialisation",
                        render: (val) => mwb_titleCase(mwb_specialisationMap[val])               
                    }
                ];
                mwb_createTable("mwb-skill-table", columns, dataSet);
                mwb_skill_init = true;
            }
        });
    }
    $(".mwb-panel").hide();
    $(`#mwb-skill-table`).show();
}