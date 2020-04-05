import QtQuick 2.6
import QtQuick.Controls 2.3
import QtQuick.Layouts 1.3
import QtQuick.Dialogs 1.2
import sunrise 1.1
import "main.js" as Main

ApplicationWindow {
    id: applicationWindow
    visible: true
    width: 640
    height: 480
    title: qsTr("Sunrise Launcher v0.0.1")
    background: Rectangle {
	    color: "black" 
	    Image {
            anchors.right: parent.right
            anchors.rightMargin: 0
            anchors.left: parent.left
            anchors.leftMargin: 0
		    sourceSize.width: 611
		    sourceSize.height: 175
		    fillMode: Image.TileHorizontally
		    verticalAlignment: Image.AlignLeft
		    source: "skins/herocity/images/background_top.png" 
		    }
	}

	FontLoader {
		id: fontMont 
		source: "skins/herocity/fonts/FontsFree-Net-mont.ttf"
    }

    ServerList {
        id: serverlist
    }

    Frame {
        id: pane_servers
        enabled: true
        visible: true
        anchors.right: parent.right
        anchors.rightMargin: 10
        anchors.left: parent.left
        anchors.leftMargin: 10
        anchors.top: parent.top
        anchors.topMargin: 10
        anchors.bottom: pane_bottom.top
        anchors.bottomMargin: 10
		background: Rectangle {
		    color: "black" 
		    opacity: 0.5
		    border.color: "#14466e"
		    border.width: 4
		}

        ButtonGroup { 
            id: buttongroup_servers 
            onClicked: Main.select(button.manifestURL);
        }

        ScrollView {
            id: scrollview_servers
            anchors.fill: parent
            contentHeight: column_servers.height + button_add.height + 4
            clip: true
            ScrollBar.horizontal.policy: ScrollBar.AlwaysOff
            ScrollBar.vertical.policy: ScrollBar.AlwaysOn

            ColumnLayout {
                id: column_servers
                spacing: 2
                anchors.top: parent.top
                anchors.topMargin: 0
                anchors.right: parent.right
                anchors.rightMargin: 0
                anchors.left: parent.left
                anchors.leftMargin: 0

                Repeater {
                    id: repeaterServers

                    Item {
                        height: 48
                        RoundButton {
                            id: button_config
                            width: 34
                            height: 34
                            radius: 16
                            leftPadding: 0
							rightPadding: 0
							topPadding: 0
							bottomPadding: 0
                            visible: button_server.checked
                            anchors.verticalCenter: parent.verticalCenter
                            anchors.left: parent.left
                            anchors.leftMargin: 0
                            icon.source: "skins/herocity/images/configure_server.png"
							icon.width: width
							icon.height: height
							icon.color: "transparent"
							background: Rectangle {
							    color: "transparent"
							}
                            onClicked: Main.configOpen(modelData.manifestURL);
                        }

                        RoundButton {
                            id: button_remove
                            width: 34
                            height: 34
                            radius: 16
                            leftPadding: 0
							rightPadding: 0
							topPadding: 0
							bottomPadding: 0
                            visible: button_server.checked
                            anchors.verticalCenter: parent.verticalCenter
                            anchors.left: button_config.right
                            anchors.leftMargin: 2
                            icon.source: "skins/herocity/images/remove_server.png"
							icon.width: width
							icon.height: height
							icon.color: "transparent"
							background: Rectangle {
								color: "transparent"
							}
                            onClicked: Main.removeOpen(modelData.manifestURL);
                        }

                        RadioButton {
                            id: button_server
                            y: 10
                            text: modelData.title
                            ButtonGroup.group: buttongroup_servers
                            anchors.verticalCenter: parent.verticalCenter
                            anchors.left: button_remove.right
                            anchors.leftMargin: 4
                            indicator: Item {}
                            checked: (modelData.manifestURL === serverlist.selected)
                            property string manifestURL: modelData.manifestURL

                            contentItem: Text {
                                text: button_server.text
                                font.family: fontMont.name
                                font.pointSize: 14
                                color: "white"
                                verticalAlignment: Text.AlignVCenter
                                leftPadding: button_server.indicator.width + button_server.spacing
                            }
                        }
                    }
                }
            }

            RoundButton {
                id: button_add
				display: AbstractButton.IconOnly
                width: 34
                height: 34
                radius: 16
				leftPadding: 0
				rightPadding: 0
				topPadding: 0
				bottomPadding: 0
                anchors.top: column_servers.bottom
                anchors.topMargin: 2
                anchors.left: parent.left
                anchors.leftMargin: 36
				icon.width: width
				icon.height: height
				icon.color: "transparent"
				background: Rectangle {
								color: "transparent"
								}
				icon.source: "skins/herocity/images/add_server.png"
                onClicked: Main.configNew();
            }
        }
    }

    Pane {
        id: pane_bottom
        height: 48
        anchors.bottom: parent.bottom
        anchors.bottomMargin: 0
        anchors.right: parent.right
        anchors.rightMargin: 0
        anchors.left: parent.left
        anchors.leftMargin: 0
        background: Rectangle {
			color: "transparent" 
		}

        Button {
            id: button_refresh
			display: AbstractButton.IconOnly
			width: 98
            height: 28
			leftPadding: 0
			rightPadding: 0
			topPadding: 0
			bottomPadding: 0
            anchors.bottom: parent.bottom
            anchors.bottomMargin: 0
            anchors.left: parent.left
            anchors.leftMargin: 0
			icon.source: "skins/herocity/images/refresh.png"
			icon.width: width
			icon.height: height
			icon.color: "transparent"
            opacity: (enabled ? 1 : .25)
				background: Rectangle {
				color: "black"
				implicitWidth: 98
				implicitHeight: 28
				border.color: "transparent"
				border.width: 0
				}
            onClicked: Main.refreshServers();
        }

        Frame {
            id: paneProgress
            visible: false
            anchors.bottom: parent.bottom
            anchors.bottomMargin: -10
            anchors.left: button_refresh.right
            anchors.leftMargin: 4
            anchors.right: button_verify.left
            anchors.rightMargin: 4
				background: Rectangle {
				color: "transparent"
				border.color: "#14466e"
				border.width: 0
			}

            ColumnLayout {
                spacing: 4
                anchors.left: parent.left
                anchors.leftMargin: 0
                anchors.right: parent.right
                anchors.rightMargin: 0

                Text {
                    id: textProgress
                    color: "white"
					font.family: fontMont.name
                }

                ProgressBar {
                    id: progressBar
                    height: 12
                    value: 0
                    Layout.fillWidth: true
                }
            }
        }

        Button {
            id: button_verify
			display: AbstractButton.IconOnly
			width: 98
            height: 28
			leftPadding: 0
			rightPadding: 0
			topPadding: 0
			bottomPadding: 0
            anchors.bottom: parent.bottom
            anchors.bottomMargin: 0
            anchors.right: button_start.left
            anchors.rightMargin: 4
			icon.source: "skins/herocity/images/verify_files.png"
			icon.width: width
			icon.height: height
			icon.color: "transparent"
            opacity: (enabled ? 1 : .25)
				background: Rectangle {
				color: "transparent"
				implicitWidth: 98
				implicitHeight: 28
				border.color: "transparent"
				border.width: 0
				}
            onClicked: Main.verifyServer();
        }

        Button {
            id: button_start
			display: AbstractButton.IconOnly
			width: 98
            height: 28
			leftPadding: 0
			rightPadding: 0
			topPadding: 0
			bottomPadding: 0
            anchors.bottom: parent.bottom
            anchors.bottomMargin: 0
            anchors.right: parent.right
            anchors.rightMargin: 0
			icon.source: "skins/herocity/images/login.png"
			icon.width: width
			icon.height: height
			icon.color: "transparent"
            opacity: (enabled ? 1 : .25)
				background: Rectangle {
				color: "transparent"
				implicitWidth: 98
				implicitHeight: 28
				border.color: "transparent"
				border.width: 0
				}
            onClicked: Main.startServer();
        }
    }

    Dialog {
        id: dialog_config
        standardButtons: StandardButton.Save | StandardButton.Cancel
        modality: Qt.WindowModal
        property string manifestURL
        onAccepted: Main.configSave();
        width: 500
        
        GridLayout {
            
            anchors.left: parent.left
            anchors.leftMargin: 0
            anchors.right: parent.right
            anchors.rightMargin: 0

            columns: 2

            Text {
                text: "Manifest URL"
                font.family: fontMont.name
            }

            TextField {
                placeholderText: "ex: https://example.com/servername/manifest"
                id: textfield_manifesturl
                Layout.fillWidth: true
            }

            Text {
                text: "Install Path"
                font.family: fontMont.name
            }

            TextField {
                placeholderText: "ex: servername or C:/CoH"
                font.family: fontMont.name
                id: textfield_installpath
                Layout.fillWidth: true
            }
        }
    }

    MessageDialog {                             
        id: dialog_remove
        title: "Remove Server"
        modality: Qt.WindowModal
        standardButtons: StandardButton.Yes | StandardButton.No
        informativeText: "This will not remove any installed files."
        property string manifestURL
        onYes: Main.removeServer();
    }

    MessageDialog {
        id: dialogError
        title: "Error"
        modality: Qt.WindowModal
        standardButtons: StandardButton.Ok
        informativeText: "See log.txt for details"
    }

    Component.onCompleted: Main.init()
    onClosing: serverlist.save("./servers.json");
}