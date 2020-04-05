
var state_enum = {
    unchecked: 0,
    ready: 1,
    updating: 2,
    error: 3
};

function init() {
    button_refresh.enabled = false;
    button_verify.enabled = false;
    button_start.enabled = false;
    serverlist.update.connect(refreshAll);
    serverlist.message.connect(showMessage);
    serverlist.progress.connect(refreshProgress);
    var task = serverlist.loadAsync("./servers.json");
    Net.await(task, function () {
        button_refresh.enabled = true;
        if (serverlist.servers.count == 0) {
            configNew();
        }
    });
}

function refreshAll() {
    refreshList();
    refreshSelected();
}

function select(manifestURL) {
    serverlist.selected = manifestURL;
    refreshSelected();
}

function refreshList() {
    repeaterServers.model = Net.toListModel(serverlist.servers);
}

function refreshSelected() {
    var server = serverlist.get(serverlist.selected);

    if (server == null || server.state !== state_enum.ready) {
        button_start.enabled = false;
        button_verify.enabled = false;
    } else if (server.state === state_enum.ready) {
        button_start.enabled = true;
        button_verify.enabled = true;
    }

    if (server != null) {
        refreshProgress(server.manifestURL, server.taskName, server.taskDone, server.taskCount)
    } else {
        refreshProgress(null, null, 0, 0)
    }
    
}

function inspect(obj) {
    for (var name in obj) {
        console.log(name);
    }
}

function verifyServer() {
    button_verify.enabled = false;
    button_start.enabled = false;
    var task = serverlist.verifyAsync();
    Net.await(task, function () {
        refreshSelected();
    });
}

function refreshServers() {
    button_refresh.enabled = false;
    button_verify.enabled = false;
    button_start.enabled = false;
    var task = serverlist.refreshAsync();
    Net.await(task, function () {
        button_refresh.enabled = true;
    });
    refreshSelected();
}

function startServer() {
    button_verify.enabled = false;
    button_start.enabled = false;
    applicationWindow.lower();
    serverlist.launch();
    refreshSelected();
}

function removeOpen(manifestURL) {
    var server = serverlist.get(manifestURL);
    if (server == null) return;

    dialog_remove.manifestURL = manifestURL;
    dialog_remove.text = "Are you sure you wish to remove server '" + server.title + "'?";
    dialog_remove.visible = true;
}

function removeServer() {
    serverlist.remove(dialog_remove.manifestURL);
    refresh();
}

function configOpen(manifestURL) {
    var server = serverlist.get(manifestURL);
    if (server == null) return;

    textfield_manifesturl.text = server.manifestURL;
    textfield_installpath.text = server.installPath;
    dialog_config.manifestURL = manifestURL;
    dialog_config.title = server.title;
    dialog_config.visible = true;
}

function configNew() {
    textfield_manifesturl.text = "";
    textfield_installpath.text = "";
    dialog_config.manifestURL = "";
    dialog_config.title = "New Server";
    dialog_config.visible = true;
}

function configSave() {
    if (dialog_config.manifestURL === "") {
        serverlist.addAsync(textfield_manifesturl.text, textfield_installpath.text);
        return;
    }
    serverlist.configAsync(dialog_config.manifestURL, textfield_manifesturl.text, textfield_installpath.text);
}

function showError(error) {
    if (error == null) return;
    dialogError.text = error;
    dialogError.visible = true;
}

function showMessage(msg) {
    dialogError.text = msg;
    dialogError.visible = true;
}

function refreshProgress(manifestURL, taskName, taskDone, taskCount) {
    if (serverlist.selected == null || taskName == "") {
        paneProgress.visible = false;
        return;
    }

    if (serverlist.selected != manifestURL)
        return;

    if (taskCount == 0) {
        progressBar.indeterminate = true;
    } else {
        progressBar.indeterminate = false;
        progressBar.value = taskDone;
        progressBar.to = taskCount;
    }

    textProgress.text = taskName;
    paneProgress.visible = true;
}