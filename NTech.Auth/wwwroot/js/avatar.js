var avatarCropper;

export function CreateCropper() {
    const image = document.getElementById('image');
    const input = document.getElementById('input');

    var files = input.files;
    var done = function (url) {
        input.value = '';
        image.src = url;
    };
    var reader;
    var file;

    if (files && files.length > 0) {
        file = files[0];

        if (URL) {
            done(URL.createObjectURL(file));
        } else if (FileReader) {
            reader = new FileReader();
            reader.onload = function (e) {
                done(reader.result);
            };
            reader.readAsDataURL(file);
        }
    }

    avatarCropper = new Cropper(image, {
        aspectRatio: 1,
        viewMode: 3,
        movable: true,
        zoomable: true,
        autoCropArea: 1,
        cropBoxMovable: false,
        cropBoxResizable: false,
        rotatable: false,
        dragMode: "move",
        toggleDragModeOnDblclick: false,
        center: false,
        guides: false,
        highlight: false
    });
}

export function DestroyCropper() {
    if (avatarCropper) {
        avatarCropper.destroy();
        avatarCropper = null;
    }
}

export function SaveImage() {
    var canvas = avatarCropper.getCroppedCanvas({
        width: 512,
        height: 512,
    });

    canvas.toBlob(function (blob) {
        var formData = new FormData();

        formData.append('file', blob, "avatar.png");

        fetch("api/account/avatar", {
            method: "POST",
            body: formData
        }).then(res => {
            if (res.ok) {
                const avatarImages = document.querySelectorAll(".avatar-img");
                var croppedImage = canvas.toDataURL("image/png");
                avatarImages.forEach(function (img) {
                    img.src = croppedImage;
                });
            }
        });

        //const avatarImages = document.querySelectorAll(".avatar-img");
        //var croppedImage = canvas.toDataURL("image/png");
        //avatarImages.forEach(function (img) {
        //    img.src = croppedImage;
        //});
    });
}
