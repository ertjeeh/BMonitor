var information = "";
var instances;
var currentInstanceId;

var cards = [];
var timers = [];
var timerTimeout;

var earliestDateRegex = /\^EarliestRefresh:([^\^]*)\^/;
var graphRegex = /GenGraph:([^\^]+)\^(.*)\^/;

var mainGridStack;
var savedGrids;

var apiLocation = "/bmonitorapi";
var staticLocation = "/bmonitorstatic";

$(document).ready(function () {
    LoadSavedGrids();
});

function LoadSavedGrids() {
    var gridCookie = getCookie("SavedGrids");
    if (gridCookie.length == 0) {
        savedGrids = [];
        return;
    }

    var savedGridsObj = JSON.parse(gridCookie);
    savedGrids = savedGridsObj;
}

function SaveSavedGrids() {
    var newValue = JSON.stringify(savedGrids);
    setCookie("SavedGrids", newValue);
}

function GetInformation(onDone) {
    $.get(apiLocation + "/Information")
        .done(function (data) {
            information = data;
            if (information.InstanceName !== "---n/a---") {
                onDone();
            } else {
                ShowGiveNameDialog();
            }
        });
}

function ShowGiveNameDialog() {
    $("#main_dialog_content").html(CONST_initializeBMonitorDialog);
    $("#main_dialog").dialog();
}

function NameBMonitor() {
    var name = $("#input_newName").val();
    if (name.length == 0) {
        DialogError("Please enter a name with length > 0");
        return;
    }

    $.ajax({
        url: apiLocation + "/Information",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Name: name }),

        success: function () {
            RefreshInstances();
            DialogInfo("BMonitor named!");
            $("#submit_newName").prop("disabled", true);
            setTimeout(RefreshPage, 500)
        },

        error: function (data) {
            DialogError(data.responseText);
        }
    });
}

function RefreshPage() {
    location.reload();
}

function GetInstances(onDone) {
    $.get(apiLocation + "/Instance")
        .done(function (data) {
            instances = data;
            onDone();
        });
}

function RefreshInformation() {
    GetInformation(function () {
        $("#sidebar_title").html(information.InstanceName)
        $("#sidebar_title_small").html(information.InstanceName)
        document.title = "BMonitor - " + information.InstanceName;
    });
}

function RefreshInstances(onDone = function () { }) {
    GetInstances(function () {
        var html = "";
        instances.forEach(instance => {
            var row = `<a href="#" onclick="ShowInstance(${instance.id})" class="w3-bar-item w3-button w3-hover-white">${instance.name}</a>\r\n`;
            html += row;
        });

        $("#main_nav_links").html(html);
        onDone();
    });
}

function InitializeBMonitor() {
    RefreshInformation();
    RefreshInstances();
}

function CloseCard(cid) {
    $.ajax({
        url: apiLocation + "/Instance/Card",
        type: "DELETE",
        contentType: "application/json",
        data: JSON.stringify({ InstanceId: currentInstanceId, CardId: cid }),

        success: function () {
            RefreshInstances(function () { ShowInstance(currentInstanceId) });
        },

        error: function (data) {
            // todo make general error position
        }
    });
}

function GenerateHtmlCard(cid) {
    $.get(apiLocation + "/Card/Html", { "cardId": cid })
        .done(function (c) {
            var html = `<h3 style="margin-top: 3px;">${c.title}</h3>
<div style="position: absolute; right: 0; top: 0"><a href="#" onclick="EditCard(${c.id})"><span class="neu-edit-ui"></span></a><a href="#" onclick="CloseCard(${c.id})"><span class="ui-icon ui-icon-closethick"></span></a></div>
<div>${c.html}</div>`
            $(`#gridcard_${cid}_content`).html(html);

            if (c.html.includes('GenGraph')) {
                var result = graphRegex.exec(c.html);
                var graphName = result[1];
                new Chart(
                    document.getElementById('c_' + graphName),
                    {
                        type: 'line',
                        data: {
                            datasets: [{
                                data: JSON.parse(result[2])
                                //data: result[2]
                            }]
                        },
                        options: {
                            scales: {
                                x: {
                                    type: 'time',
                                    time: {
                                        unit: 'hour',
                                        displayFormats: {
                                            'hour': 'HH:MM'
                                        }
                                    }
                                }
                            },
                            plugins: {
                                legend: {
                                    display: false
                                }
                            },
                            maintainAspectRatio: false,
                        }
                    }
                );
            }

            if (!c.html.includes('EarliestRefresh')) return;
            var earliestRefreshDateStr = earliestDateRegex.exec(c.html)[1];
            if (typeof earliestRefreshDateStr === 'undefined') { return; }

            var earliestRefreshDate = new Date(earliestRefreshDateStr);
            var currentDate = new Date();
            timers.push([cid, "HtmlCard", earliestRefreshDate, currentDate]);
        });
}

function EditCard(id) {
    $.get(apiLocation + "/Card/Html", { "cardId": id, "applyTransformations": false })
        .done(function (c) {
            OpenAddCardDialog();

            $("#input_newCardTitle").val(c.title);
            $("#textarea_newCardHtml").val(c.html.replace(/<!--[\s\S]*?-->/g, ''));
            $("#submit_newCard").val("Update");
            $("#submit_newCard").attr("onclick", "DoEditCard(" + id + ")");
        });
}

function DoEditCard(id) {
    var newTitle = $("#input_newCardTitle").val();
    var newHtml = $("#textarea_newCardHtml").val();

    $.ajax({
        url: apiLocation + "/Card/Html/" + id,
        type: "PATCH",
        contentType: "application/json",
        data: JSON.stringify({ Title: newTitle, Html: newHtml }),

        success: function () {
            setTimeout(ShowInstance(currentInstanceId), 250);
            DialogInfo("Card updated.");
            $("#submit_newCard").prop("disabled", true);
        },

        error: function (data) {
            DialogError(data.responseText);
        }
    });
}

function RefreshCard(card) {
    var [id, type] = card;
    $(`#gridcard_${id}_content`).html("Loading...");
    if (type == "HtmlCard") {
        GenerateHtmlCard(id);
    }
    $(`#gridcard_${id}_progress`).progressbar({ value: 100 });
}

function EnableGridSave(show = true) {
    if (show) {
        $("#saveGridLink").show();
    } else {
        $("#saveGridLink").hide();
    }
}

function ShowCards() {
    var instance = instances.filter(i => i.id == currentInstanceId)[0];

    cards = [];
    timers = [];

    var items = instance.cards.map(function (c) {
        cards.push([c.id, c.type]);
        return { id: c.id, w: 3, h: 4, content: `<div id="gridcard_${c.id}"><div id="gridcard_${c.id}_content"></div><div id="gridcard_${c.id}_status" class="gridcardstatus"><div id="gridcard_${c.id}_progress"></div></div></div>` }
    });

    mainGridStack = GridStack.init({
        cellHeight: 25,
        cellWidth: 25,
        disableOneColumnMode: true
    });
    mainGridStack.load(items);
    mainGridStack.on('change', function (event, items) {
        EnableGridSave();
    });
    LoadGrid();

    cards.forEach(c => {
        RefreshCard(c);
    });
}

function ShowInstance(id) {
    currentInstanceId = id;
    clearTimeout(timerTimeout)
    var instance = instances.filter(i => i.id == id)[0];
    $("#other_content").hide();
    $("#grid-stack").show();

    $("#main_content_title").html(instance.name);

    ShowCards();

    timerTimeout = setTimeout(UpdateCards, 500);

    $("#main_content_top").html(`<a href="#" onclick="OpenAddCardDialog()"><span class="neu-add-sm iconcolor"></span><span>Add card...</span></a>
&nbsp;
<a id="saveGridLink" href="#" onclick="SaveGrid()" style="display: none"><span class=""neu-save"">Save layout...</span></a>`
    );
}

function SaveGrid() {
    var newSavedGrid = JSON.stringify(mainGridStack.save(false));
    var newEntry = { instanceId: currentInstanceId, grid: newSavedGrid };
    var indexInArr = savedGrids.findIndex(s => s.instanceId === currentInstanceId);
    if (indexInArr == -1) {
        savedGrids.push(newEntry);
    } else {
        savedGrids[indexInArr] = newEntry;
    }

    // todo notify user it's saved
    SaveSavedGrids();
    EnableGridSave(false);
}

function LoadGrid() {
    var savedGrid = savedGrids.filter(s => s.instanceId == currentInstanceId)[0];
    if (typeof savedGrid === 'undefined') {
        return;
    }

    var savedGridObj = JSON.parse(savedGrid.grid);
    mainGridStack.load(savedGridObj, false);
}

function UpdateCards() {
    var timersDone = [];
    timers.forEach(t => {
        if (UpdateCard(t)) {
            timersDone.push(t);
        }
    });

    timersDone.forEach(t => {
        // remove timer from timers
        timers = timers.filter(el => { return el[0] != t[0] });
        RefreshCard([t[0], t[1]]);
    });

    timerTimeout = setTimeout(UpdateCards, 500);
}

function UpdateCard(timer) {
    var [cid, type, earliestRefreshDate, currentDateFromTimer] = timer;

    var currentDate = new Date();
    var totalMs = earliestRefreshDate.getTime() - currentDateFromTimer.getTime();
    var msToDate = earliestRefreshDate.getTime() - currentDate.getTime();
    var perc = (msToDate / totalMs) * 100;
    $(`#gridcard_${cid}_progress`).progressbar({ value: perc });
    return msToDate < -500;
}

function AddInstance() {
    var newName = $("#input_newInstanceName").val();

    $.ajax({
        url: apiLocation + "/Instance",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Name: newName, Description: "" }),

        success: function () {
            RefreshInstances();
            DialogInfo("Card added.");
            $("#submit_newInstance").prop("disabled", true);
        },

        error: function (data) {
            DialogError(data.responseText);
        }
    });
}

function OpenAddInstanceDialog() {
    $("#main_dialog_content").html(CONST_addInstanceDialog);
    $("#main_dialog").dialog();
}

function AddCard() {
    var newTitle = $("#input_newCardTitle").val();
    var newHtml = $("#textarea_newCardHtml").val();

    $.ajax({
        url: apiLocation + "/Card/Html",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ InstanceId: currentInstanceId, Title: newTitle, Html: newHtml }),

        success: function () {
            RefreshInstances(function () { ShowInstance(currentInstanceId) });
            DialogInfo("Card added.");
            $("#submit_newCard").prop("disabled", true);
        },

        error: function (data) {
            DialogError(data.responseText);
        }
    });
}

function OpenMonitorsPage() {
    $("#grid-stack").hide();
    $("#other_content").show();
    $("#main_content_top").html(`<a href="#" onclick="OpenAddMonitorDialog()"><span class="neu-add-sm iconcolor"></span><span>Add monitor...</span></a>`);
    RefreshMonitorsPage();
}

function RefreshMonitorsPage() {
    $.get(staticLocation + "/monitors.html")
        .done(function (c) {
            $("#other_content").html(c);
            InitPage();
        });
}

function OpenAddCardDialog() {
    $("#main_dialog_content").html(CONST_addCardDialog);
    OpenNewCardForm();
    $("#main_dialog").dialog({ width: 'auto' });
}

function OpenNewCardForm() {
    var cardType = $('#select_newCardType').find(":selected").text();
    if (cardType == "HtmlCard") {
        $("#addcard_form").html(CONST_addHtmlCardForm);
    }
}

function OpenAddMonitorDialog(afterOpen = undefined) {
    $.get(staticLocation + "/addmonitordialog.html")
        .done(function (c) {
            $("#main_dialog_content").html(c);
            $("#main_dialog").dialog({ width: 'auto' });

            if (afterOpen != undefined) {
                afterOpen();
            } else {
                InitializeAddDialog();
            }
        });
}

function DialogError(msg) {
    $("#addcard_dialog_message").html(`
<div class="ui-state-error" style="padding: 0 .7em;">
    <p><span class="ui-icon ui-icon-info" style="float: left; margin-right: .3em;"></span>${msg}</p>
</div>`);
}

function DialogInfo(msg) {
    $("#addcard_dialog_message").html(`
<div class="ui-state-highlight" style="padding: 0 .7em;">
    <p><span class="ui-icon ui-icon-alert" style="float: left; margin-right: .3em;"></span>${msg}</p>
</div>`);
}

function getCookie(cname) {
    let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function setCookie(cname, cvalue, exdays = 1825) {
    const d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    let expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

var CONST_initializeBMonitorDialog = `
<p>
    New BMonitor instance found, please give it a name: <br/>
    <form>
        <input type="text" id="input_newName" placeholder="BMonitor Instance name"/><br/><br/>
        <input type="button" id="submit_newName" value="Submit" onclick="NameBMonitor()"/>
    </form>
</p>
<div id="addcard_dialog_message"></div>
`;

var CONST_addInstanceDialog = `
<p>
    <form>
        <input type="text" id="input_newInstanceName" placeholder="New instance name"/><br/>
        <input type="button" id="submit_newInstance" value="Create" onclick="AddInstance()"/>
    </form>
</p>
<div id="addcard_dialog_message"></div>
`;

var CONST_addCardDialog = `
<p>
    <form>
        <div>
            <select id="select_newCardType" onchange="OpenNewCardForm()">
                <option value="HtmlCard" selected>HtmlCard</option>
            </select>
            <br/>
        </div>
        <div id="addcard_form">

        </div>
        <div><input type="button" id="submit_newCard" value="Create" onclick="AddCard()"/></div>
    </form>
</p>
<div id="addcard_dialog_message"></div>
`;

var CONST_addHtmlCardForm = `
<input type="text" id="input_newCardTitle" placeholder="New card title"/><br/><br/>
<textarea id="textarea_newCardHtml" placeholder="Enter your new card HTML here" rows="6" cols="50"/>
`;