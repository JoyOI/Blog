function DropEnable() {
    $('.markdown-textbox').unbind().each(function () {
        var editor = $(this);
        if (editor[0].smde == undefined) {
            var smde = new SimpleMDE({
                element: editor[0],
                spellChecker: false,
                status: false
            });
            editor[0].smde = smde;
            var begin_pos, end_pos;
            $(this).parent().children().unbind().dragDropOrPaste(function () {
                begin_pos = smde.codemirror.getCursor();
                smde.codemirror.setSelection(begin_pos, begin_pos);
                smde.codemirror.replaceSelection(replaceText);
                begin_pos.line++;
                end_pos = { line: begin_pos.line, ch: begin_pos.ch + replaceInnerText.length };
            },
                function (result) {
                    smde.codemirror.setSelection(begin_pos, end_pos);
                    smde.codemirror.replaceSelection('![' + result.FileName + '](/file/download/' + result.Id + ')');
                });
        }
    });
}

function savePost(url) {
    $.post('/admin/post/edit', {
        __RequestVerificationToken: $('#frmSavePost input[name="__RequestVerificationToken"]').val(),
        title: $('#txtTitle').val(),
        id: url,
        newId: $('#txtUrl').val(),
        content: $('#txtContent')[0].smde.value(),
        tags: $('#txtTags').val(),
        catalog: $('#lstCatalogs').val(),
        isPage: $('#chkIsPage').is(':checked')
    }, function (html) {
        $('.post-body').html(html);
        $('.post-edit').slideUp();
        $('.post-body').slideDown();
        Highlight();
        popResult('文章保存成功');
    });
}

function editCatalog(id) {
    var parent = $('tr[data-catalog="' + id + '"]');
    parent.find('.display').hide();
    parent.find('.editing').fadeIn();
}

function cancelEditCatalog() {
    $('.editing').hide();
    $('.display').fadeIn();
}

function deleteCatalog(id) {
    var parent = $('tr[data-catalog="' + id + '"]');
    parent.remove();
    $.post('/admin/catalog/delete', {
        id: id,
        __RequestVerificationToken: $('#frmDeleteCatalog input[name="__RequestVerificationToken"]').val()
    }, function () {
        popResult('删除成功');
    });
}

function saveCatalog(id) {
    var parent = $('tr[data-catalog="' + id + '"]');
    $.post('/admin/catalog/edit/', {
        id: id,
        __RequestVerificationToken: $('#frmEditCatalog input[name="__RequestVerificationToken"]').val(),
        newId: parent.find('.title').val(),
        title: parent.find('.title-zh').val(),
        order: parent.find('.order').val()
    }, function () {
        parent.find('.d-title').html(parent.find('.title').val());
        parent.find('.d-title-zh').html(parent.find('.title-zh').val());
        parent.find('.d-order').html(parent.find('.order').val());
        parent.find('.editing').hide();
        parent.find('.display').fadeIn();
        popResult('修改成功');
    });
}

function saveConfig() {
    $.post('/admin/index', $('#frmConfig').serialize(), function () {
        popResult('网站配置信息修改成功');
    });
}

function popResult(txt) {
    var msg = $('<div class="msg hide">' + txt + '</div>');
    msg.css('left', '50%');
    $('body').append(msg);
    msg.css('margin-left', '-' + parseInt(msg.outerWidth() / 2) + 'px');
    msg.removeClass('hide');
    setTimeout(function () {
        msg.addClass('hide');
        setTimeout(function () {
            msg.remove();
        }, 400);
    }, 2600);
}

function showQrCode() {
    $('.qrcode').addClass('qrcode-active');
}

function uploadAttachment() {
    $('#uploadFile').unbind('change').change(function () {
        uploadFile();
    });
    $('#uploadFile').click();
}

function uploadFile() {
    var formData = new FormData($('#frmAjaxUpload')[0]);
    var editor = $('.markdown-textbox');
    var smde = editor[0].smde;

    var begin_pos, end_pos;

    begin_pos = smde.codemirror.getCursor();
    smde.codemirror.setSelection(begin_pos, begin_pos);
    smde.codemirror.replaceSelection(replaceText);
    begin_pos.line++;
    end_pos = { line: begin_pos.line, ch: begin_pos.ch + replaceInnerText.length };

    $.ajax({
        url: '/file/upload',
        type: 'POST',
        data: formData,
        dataType: 'json',
        async: false,
        cache: false,
        contentType: false,
        processData: false,
        success: function (result) {
            smde.codemirror.setSelection(begin_pos, end_pos);
            smde.codemirror.replaceSelection('![' + result.FileName + '](/file/download/' + result.Id + ')');
        },
        error: function (returndata) {
        }
    });
}

$(document).ready(function () {
    var url = $('#qrcode').attr('data-url');
    $('#qrcode').qrcode(url);
    $(window).click(function (e) {
        if ($('.qrcode-active').length > 0) {
            if ($(e.target).parents('.qrcode').length == 0 && !$(e.target).hasClass('qrcode'))
                $('.qrcode').removeClass('qrcode-active');
        }
    });

    // Binding blog roll
    $('.sidebar-blog-roll').hover(function () {
        var name = $(this).find('img').attr('alt');
        if (!name)
            return;
        var pos = $(this).position();
        var top = pos.top + $(this).outerHeight();
        var left = pos.left + $(this).outerWidth() / 2;
        $('.sidebar-blog-roll-tip').css('top', top);
        $('.sidebar-blog-roll-tip').css('left', left);
        $('.sidebar-blog-roll-tip').text(name);
        $('.sidebar-blog-roll-tip').addClass('active');
    }, function () {
        $('.sidebar-blog-roll-tip').removeClass('active');
    });
});

function toggleChatBox() {
    if ($('.chat-iframe').hasClass('active')) {
        $('.chat-iframe').removeClass('active');
        $('#back-to-top').css('margin-right', 0);
    } else {
        $('.chat-iframe').addClass('active');
        $('#back-to-top').css('margin-right', 350);
    }
}

function DropEnable() {
    $('.markdown-editor').unbind().each(function () {
        var editor = $(this);
        if (!editor[0].smde) {
            var smde = new SimpleMDE({
                element: editor[0],
                spellChecker: false,
                status: false,
                content: $(editor[0]).val()
            });
            setTimeout(function () { $('.CodeMirror').click(); }, 300);
            editor[0].smde = smde;
            var begin_pos, end_pos;
            $(this).parent().children(".CodeMirror").unbind().dragDropOrPaste(function () {
                begin_pos = smde.codemirror.getCursor();
                smde.codemirror.setSelection(begin_pos, begin_pos);
                smde.codemirror.replaceSelection(replaceText);
                begin_pos.line++;
                end_pos = { line: begin_pos.line, ch: begin_pos.ch + replaceInnerText.length };
            },
                function (result) {
                    smde.codemirror.setSelection(begin_pos, end_pos);
                    smde.codemirror.replaceSelection('![' + result.FileName + '](/file/download/' + result.Id + ')');
                });
        }
    });
}