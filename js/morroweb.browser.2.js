function mw_newTable(id) {
    return $(`<table id='mw_${id}_table' class='mwb-table table-sm table-borderless display nowrap table table-compact table-striped w-100 responsive'></table>`);
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
        scrollY: "calc(100vh - 320px)",
        scrollX: true,
        sScrollX: "100%"
    });

    // Initialise Bootstrap tooltips.
    var tooltipTriggerList = $(`#mw_${id}_table [data-bs-toggle="tooltip"]`);
    [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))
}

function mw_showPanel(container) {
    $(".mwb-panel").hide();
    $(`#${container}`).show();
}

var mw_gmst_init = false;
function mw_gmst_browser(container) {
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
    mw_showPanel(container);
}

var mw_global_init = false;
function mw_global_browser(container) {
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
    mw_showPanel(container);
}
