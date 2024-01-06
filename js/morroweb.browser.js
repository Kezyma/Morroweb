function mwb_init() {
    
}

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

function mwb_attributeIcon(attribute) {
    return `<img src='img\\icons\\attribute_${attribute.toLowerCase()}.png' data-bs-toggle="tooltip" data-bs-title='${attribute}' />`;
}

function mwb_skillIcon(skill) {
    return `<img src='img\\icons\\${skill.toLowerCase()}.png' data-bs-toggle="tooltip" data-bs-title='${skill}' />`;
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
                    { title: "Id" },
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
                    { title: "Playable" }
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
                    { title: "Id" },
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
                        render: (data) => data.map((attrObj) => (attrObj != "None") ? mwb_skillIcon(attrObj) : "").join(""),
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
