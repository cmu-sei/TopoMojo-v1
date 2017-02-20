/*
 *  handjam-common.js
 *
 *  Common functions.
 *
 *  jmattson@sei.cmu.edu
 *
 */

var _debug = true;

function debug(o) {
    if (_debug) { console.log(o); }
}

function newUUID() {
    var d = new Date().getTime();
    if(window.performance && typeof window.performance.now === "function"){
        d += performance.now(); //use high-precision timer if available
    }
    var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = (d + Math.random()*16)%16 | 0;
        d = Math.floor(d/16);
        return (c=='x' ? r : (r&0x3|0x8)).toString(16);
    });
    return uuid;
}

function handJam(object, key, value) {
    /* "hand-jam"" data handling for objects and arrays
        * if generating element names via code, format them for a generic
        * updateHandler function.
        *
        * example: key='array.2.item.0' will update object.array[2].item[0]
        */
    if (key) {
        var path = key.split('.');
        while (path.length>1) {
            object = object[path[0]];
            path.shift();
        }
        object[path[0]] = value;
    }
}

function onTextFocus() {
    $(this).one('mouseup', function() {
        $(this).select();
    });
}

jQuery["jsonPost"] = function( url, data, callback ) {
    // shift arguments if data argument was omitted
    if ( jQuery.isFunction( data ) ) {
        callback = data;
        data = undefined;
    }

    return jQuery.ajax({
        url: url,
        type: "POST",
        contentType:"application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: callback
    });
};

function confirmAction() {
    $div = $("#proto-confirm-div").clone(true).prop('id', '').show();
    var msg = $(this).attr('confirm');
    if (!msg) msg = '';
    $div.children('span').text('[Confirm] ' + msg);
    $div.children('button:first').click(function() {
        $(this).parent().prev().trigger('confirmed');
        $(this).parent().remove();
    });
    $div.children('button:last').click(function() {
        $(this).parent().remove();
    });
    $div.insertAfter($(this));
}

function pathDisplayText(path) {
    var a = path.split('/');
    var b = a[a.length-1];
    if (b.lastIndexOf('.')>=0) b = b.substring(0,b.lastIndexOf('.'));
    return b;
}

function stripHashTag(tag) {
    var x = tag.indexOf('#');
    return (x >=0) ? tag.substring(0,x) : tag;
}


/*
 *  console.html / console.js
 *
 *  A pretty simple web console.
 *
 *  jmattson@sei.cmu.edu
 *
 */

function WebConsole(id, topoid, url, conditions) {

    var wmks = null;
    var _disconnected = false;
    var VmStateOff = 0,
        VmStateRunning = 1,
        VmStateSuspended = 2;

    $(window).resize(function() {
        if (wmks) {
            var rect = wmks.getRemoteScreenSize();
            //console.log(rect + window.innerWidth + 'x' + window.innerHeight);
            var scale = window.innerWidth < rect.width || window.innerHeight < rect.height;
            wmks.setOption('rescale', scale);
            wmks.updateScreen();
        }
    });

    function init() {

        $('#console-tools-div button[name="cad"]').click(function() {
            if (wmks) {
                wmks.sendCAD();
            }
        });

        $('#console-tools-div button[name="scale"]').click(function() {
            if (wmks) {
                var scale = !$(this).data('option');
                $(this).data('option', scale);
                $(this).children('i').toggleClass('glyphicon-resize-full', !scale);
                $(this).children('i').toggleClass('glyphicon-resize-small', scale);
                wmks.setOption('rescale', scale);
            }
        });

        $('#console-tools-div button[name="fullscreen"]').click(function() {
            if (wmks && !wmks.isFullScreen() && wmks.canFullScreen()) {
                wmks.enterFullScreen();
            }
        });

        $('#console-tools-div button[name="upload"]').click(uploader);
        $('#console-tools-div button[name="iso"]').click(loadIsos);
        $('#console-tools-div button[name="net"]').click(loadNets);

        $('#feedback-div button[name="start"]').click(vmStart);

        $('#iso-upload-btn').click(uploadIso);

        $('#console-tools-btn').click(function() {
            $(this).nextAll().toggleClass('hidden');
        });

        //uiConnected();

    }

    function uiConnected() {
        $('#console-tools-btn').removeClass('hidden');
    }

    function uiDisconnected() {
        _disconnected = true;
        $('#feedback-div').children('p:first').text('Session disconnected.');
        $('#feedback-div').children('button:first').hide();
        $('#feedback-div').show();
        $('#console-tools-btn').addClass('hidden');
        $('#console-tools-div').addClass('hidden');
    }

    function uiPoweredOff() {
        $('#feedback-div').children('p:first').text('');
        $('#feedback-div').children('button:first').show();
        $('#feedback-div').show();
    }

    function uploader() {
        $(this).next().toggleClass('hidden');
    }

    function loadIsos() {
        addSelectionItems($(this).next(), 'iso');
    }

    function loadNets() {
        addSelectionItems($(this).next(), 'net');
    }

    function addSelectionItems($ul, key)
    {
        $ul.toggleClass('hidden');
        if ($ul.hasClass('hidden'))
            return;

        var $proto = $ul.find('li:first');
        $proto.removeClass('hidden');
        $proto.nextAll().remove();

        $.get('/api/vm/' + key + 'options/' + id)
        .done(function(options) {
            var items = options[key];
            for (i=0; i<items.length; i++) {
                var $newitem = $proto.clone(true).prop('id', '').removeClass('hidden');
                var $btn = $newitem.find('button');
                $btn.val(items[i])
                .data('key', key)
                .click(vmChange)
                .text(pathDisplayText(stripHashTag(items[i])));
                $ul.append($newitem);
            }
            $proto.addClass('hidden');
        });
    }

    function vmStart() {
        $(this).hide();//.addClass('pending');
        $.post('/api/vm/start/' + id)
        .fail(function(jqxhr, textStatus, err) {
            $(this).next().text('Vm failed to start.');
            $(this).removeClass('pending');
        })
        .done(function(data) {
            window.location = '';
        })
        .always(function(jqxhr) {

        });
    }

    function vmChange() {
        $('#item-selection-div').hide();
        console.log($(this).val());
        $.jsonPost('/api/vm/change/' + id, { key: $(this).data('key'), value: $(this).val()});
    }

    function uploadIso() {
        var $this = $(this);
        var $progress = $this.prev();
        var files = $('#iso-upload-input')[0].files;
        if (!files.length) return;
        //var scope = $this.parent().find('input:radio[name="scope"]:checked').val();
        var data = new FormData();
        var key = newUUID();
        $.each(files, function(i, file) {
            data.append('file', file, 'temp-'+file.name + '#' + key);
        });
        $this.prop('disabled', true);
        setTimeout(uploadProgress, 2000, $this, key);

        $.ajax({
            url: '/file/uploadiso/' + topoId,
            type: 'POST',
            data: data,
            cache: false,
            dataType: 'json',
            processData: false,
            contentType: false
        }).fail(function(jqXhr, textStatus, err)
        {
            //$this.next().next().text(jqXhr.responseJSON.message).addClass('label-danger');
            console.log(jqXhr.responseJSON);
        })
        .done(function(result) {
            console.log(result.filename);
            //loadIsos();
        })
        .always(function() {
            //$this.next().text('');
            $this.prop('disabled', false);
        });
    }

    var progressIdleCount = 0 ;
    function uploadProgress($proto, key) {
        var $progress = $proto.prev();

        $.get('/api/file/progress/' + key)
        .fail(function(jqXhr, status, error) {
            console.log(error);
        })
        .done(function(result) {
            if (result < 0) progressIdleCount += 1;
            if (result < 100) {
                $progress.text((result < 0) ? '...' : result + '%');
                if (progressIdleCount < 10) {
                    setTimeout(uploadProgress, 2000, $proto, key);
                }
            } else {
                $progress.text('').addClass('hidden');
                $proto.prop('disabled', false);
            }
        })
        .always(function() {

        });
    }

    function launch() {
        $('#feedback-div').hide();
        wmks = WMKS.createWMKS('console-canvas-div',{
            rescale: false,
            position: 0,
            changeResolution: false,
            useVNCHandshake: false
        })
        .register(WMKS.CONST.Events.CONNECTION_STATE_CHANGE, function(event,data){
            if(data.state == WMKS.CONST.ConnectionState.CONNECTED){
                console.log('connection state change : connected');
                //uiConnected();
            }
            if(data.state == WMKS.CONST.ConnectionState.CONNECTING){
                console.log('connection state change : connecting ' + data.vvc + ' ' + data.vvcSession);
            }
            if(data.state == WMKS.CONST.ConnectionState.DISCONNECTED){
                console.log('connection state change : disconnected ' + data.reason + ' ' + data.code);
                wmks.disconnect();
                wmks.destroy();
                //wmks = null;
                uiDisconnected();
            }

        })
        .register(WMKS.CONST.Events.REMOTE_SCREEN_SIZE_CHANGE, function (e, data) {
            console.log('wmks remote_screen_size_change: ' + data.width + 'x' + data.height);
            $('#console-canvas-div')[0].width=data.width;
            $('#console-canvas-div')[0].height=data.height;
        })
        .register(WMKS.CONST.Events.HEARTBEAT, function (e, data) {
            console.log('wmks heartbeat: ' + data);
        })
        .register(WMKS.CONST.Events.COPY, function (e, data) {
            console.log('wmks copy: ' + data);
        })
        .register(WMKS.CONST.Events.ERROR, function (e, data) {
            console.log('wmks error: ' + data.errorType);
        })
        .register(WMKS.CONST.Events.FULL_SCREEN_CHANGE, function (e, data) {
            console.log('wmks full_screen_change: ' + data.isFullScreen);
        });

        // wmks.connect("wss://ESXi.host.IP.Address:443/ticket/webmksTicket");
        wmks.connect(url);

    }

    function postVmAnswer() {
        var $proto = $(this);
        var q = $proto.data('question');
        $.post('/api/vm/answer/' + q.vid + '/' + q.qid + '/' + q.answer, {})
        .fail(function(jqXhr, status, error) {
            console.log('postVmAnswer: ' + error);
        })
        .done(function(result) {

        })
        .always(function(){
            $proto.parent().remove();
        });
    }

    function handleQuestion(vm) {
        var existing = $('#question-div').toArray().length > 0;
        if (vm.question) {
            if (!existing) {
                var $div = $('#proto-question-div').clone(true)
                    .prop('id', 'question-div')
                    .show()
                    .appendTo($('#console-canvas-div'));

                $div.find('span').text(vm.question.prompt);
                var $proto = $div.find('button:first');
                for (var i = 0; i < vm.question.choices.length; i++) {
                    $proto.clone().show()
                    .data('question', { vid: vm.id, qid: vm.question.id, answer: vm.question.choices[i].key})
                    .val(i)
                    .text(vm.question.choices[i].label)
                    .toggleClass('default', vm.question.defaultChoice==vm.question.choices[i].key)
                    .click(postVmAnswer)
                    .appendTo($div);
                }
            }
        } else {
            if (existing) $('#question-div').remove();
        }
    }

    function loadVm() {
        if (_disconnected)
            return;

        $.get('/api/vm/load/' + id)
        .fail(function(jqXhr, status, error){
            console.log(error);
            $('#feedback-div p:first').text('Failed to load console');
            uiDisconnected();
        })
        .done(function(vm) {
            if (!_disconnected) {
                uiConnected();
                handleQuestion(vm);
                if (!wmks) {
                    if (vm.state == VmStateRunning) {
                        $('#feedback-div button').hide();
                        $('#feedback-div p:first').text('Connected');
                        if (!mock)
                            launch();
                    }
                    else {
                        $('#feedback-div p:first').text('');
                        uiPoweredOff();
                    }
                }
            }
            if (!_disconnected)
                setTimeout(loadVm, 10000);
        })
        .always(function() {

        });
    }

    init();
    loadVm();
}