// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function loadTasks() {
    var settings = {
        "url": "/api/Tasks",
        "method": "GET",
        "timeout": 0,
        "headers": {
            "Authorization": "Bearer " + getCookie('AccessToken')
        },
    };

    $.ajax(settings).done(function (response) {

        $("#list-tab-pending").empty();
        $("#list-tab-concluded").empty();
        var pendingTasks = 0;
        var concludedTasks = 0;
        $.each(response, function (index, obj) {
            
            if (obj.concluded == true) {
                concludedTasks++;
                var targetContainer = "#list-tab-concluded";
                var itemClass = "list-group-item-success";

                $(targetContainer).append(
                    `<a title="Click to manage Task ${obj.description}" class="list-group-item list-group-item-action ${itemClass}  d-flex justify-content-between" id="list-home-list" data-toggle="list" href="#list-task-${obj.id}" role="tab" aria-controls="task-${obj.id}">
                        
                        <p class="p-0 m-0 flex-grow-1"> <i class="fas fa-check-square"></i>  ${obj.description}</p>
                        <span class="pull-right">
                            <span title="Delete task" class="btn btn-xs btn-default" onclick="showDeleteTaskModal(event,{description:'${obj.description}',id:${obj.id},concluded:${obj.concluded}})">
                                <i class="fas fa-trash-alt"></i>
                            </span>
                        </span>
                    </a>`
                );
            }
            else {
                pendingTasks++;
                var targetContainer = "#list-tab-pending";
                var itemClass = "list-group-item-warning";

                $(targetContainer).append(
                    `<a title="Click to manage Task '${obj.description}'" class="list-group-item list-group-item-action ${itemClass}  d-flex justify-content-between" id="list-home-list" data-toggle="list" href="#list-task-${obj.id}" role="tab" aria-controls="task-${obj.id}">
                         
                        <p onclick="setTaskIsConcluded(event,${obj.id})" title="Mark as concluded" class="p-0 m-0 flex-grow-1"><i class="fas fa-square"></i> ${obj.description}</p>
                        <span class="pull-right">
                            <span title="Mark as concluded" class="btn btn-xs btn-default" onclick="setTaskIsConcluded(event,${obj.id})">
                                <i class="far fa-check-circle"></i>
                            </span>
                            <span title="Update description" class="btn btn-xs btn-default" onclick="showUpdateTaskModal(event,{description:'${obj.description}',id:${obj.id},concluded:${obj.concluded}})">
                                <i class="fas fa-pencil-alt"></i>
                            </span>
                            <span title="Delete task" class="btn btn-xs btn-default" onclick="showDeleteTaskModal(event,{description:'${obj.description}',id:${obj.id},concluded:${obj.concluded}})">
                                <i class="fas fa-trash-alt"></i>
                            </span>
                        </span>
                    </a>`
                );

            }


            


        });
        if (pendingTasks == 0) {
            var targetContainer = "#list-tab-pending";
            var itemClass = "list-group-item-warning";

            $(targetContainer).append(
                `<a class="list-group-item list-group-item-action ${itemClass}  d-flex justify-content-between" id="list-home-list" data-toggle="list" href="#list-task-0" role="tab" aria-controls="task-0">
                        <p class="p-0 m-0 flex-grow-1">You have no pending tasks! <i class="fas fa-laugh-wink"></i></p>
                        
                    </a>`
            );
        }
        if (concludedTasks == 0) {
            var targetContainer = "#list-tab-concluded";
            var itemClass = "list-group-item-success";

            $(targetContainer).append(
                `<a class="list-group-item list-group-item-action ${itemClass}  d-flex justify-content-between" id="list-home-list" data-toggle="list" href="#list-task-0" role="tab" aria-controls="task-0">
                        <p class="p-0 m-0 flex-grow-1">You haven't completed any tasks yet! <i class="fas fa-flushed"></i></p>
                        
                    </a>`
            );
        }

    });
}


function showNewTaskModal(event) {
    
    $('#modal-container-new-task').modal('show');
    $("#newTaskDescription").unbind("keypress");
    $('#newTaskDescription').keypress(function (event) {
        if (event.keyCode == 13) {
            confirmNewTask();
            event.preventDefault();
        }
    });
    setTimeout(function(){
        $('#newTaskDescription').focus();
    }, 300)
    

    event.stopPropagation();
}

function showUpdateTaskModal(event, taskData) {
    $("#update-task-id").val(taskData.id);
    $("#update-task-concluded").val(taskData.concluded);
    $('#update-task-id-label').text(taskData.id);
    $('#update-task-description-label').text(taskData.description);
    $('#modal-container-update-task').modal('show');
    $("#newDescription").unbind("keypress");
    $('#newDescription').keypress(function (event) {
        if (event.keyCode == 13) {
            confirmUpdateTask();
            event.preventDefault();
        }
    });

    setTimeout(function () {
        $('#newDescription').focus();
    }, 300)
    event.stopPropagation();
}

function showDeleteTaskModal(event, taskData) {
    $("#delete-task-id").val(taskData.id);
    $('#delete-task-id-label').text(taskData.id);
    $('#delete-task-description-label').text(taskData.description);
    $('#modal-container-delete-task').modal('show');

    $("#delete-task-id").click(function () {
        confirmDeleteTask(taskData.id);
        $("#delete-task-id").unbind("click");
    });
    
    setTimeout(function () {
        $('#delete-task-id').focus();
    }, 300)
    event.stopPropagation();
}


function confirmDeleteTask(taskId) {


    var settings = {
        "url": "/api/Tasks/" + taskId,
        "method": "DELETE",
        "timeout": 0,
        "headers": {
            "Authorization": "Bearer " + getCookie('AccessToken')
        },
    };

    $.ajax(settings).done(function (response) {
        loadTasks();
        $('#modal-container-delete-task').modal('hide');
    });


}

function confirmNewTask() {
    var newTaskDescription = $('#newTaskDescription').val();
    var concluded = false;

    if (!newTaskDescription) {
        return;
    }

    var settings = {
        "url": "/api/Tasks/",
        "method": "POST",
        "timeout": 0,
        "headers": {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + getCookie('AccessToken')
        },
        "data": JSON.stringify({ "Description": newTaskDescription, "Concluded": concluded }),
    };

    $.ajax(settings).done(function (response) {
        loadTasks();
        $('#newTaskDescription').val("");
        $('#modal-container-new-task').modal('hide');
    });

}

function confirmUpdateTask() {

    var taskId = $('#update-task-id').val();
    var newDescription = $('#newDescription').val();
    var concluded = $("#update-task-concluded").val();

    if (!newDescription) {
        return;
    }

    var settings = {
        "url": "/api/Tasks/" + taskId,
        "method": "PUT",
        "timeout": 0,
        "headers": {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + getCookie('AccessToken')
        },
        "data": JSON.stringify({ "Description": newDescription, "Concluded": concluded == "true" }),
    };

    $.ajax(settings).done(function (response) {
        loadTasks();

        $('#update-task-id').val("");
        $('#newDescription').val("");
        $("#update-task-concluded").val("");
        $('#modal-container-update-task').modal('hide');
    });
}


function setTaskIsConcluded(event, taskId) {
    var settings = {
        "url": "/api/Tasks/" + taskId + "/SetConcluded",
        "method": "POST",
        "timeout": 0,
        "headers": {
            "Authorization": "Bearer " + getCookie('AccessToken')
        },
    };

    $.ajax(settings).done(function (response) {

        loadTasks();

    });
    event.stopPropagation();
}



//Source:https://www.w3schools.com/js/js_cookies.asp
//Credit for w3schools
function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function ignoreEnterKey(event) {
    if (event.which == '13') {
        return event.preventDefault();
    }
}

