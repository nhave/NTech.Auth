export function CopyToClipboard(text) {
    navigator.clipboard.writeText(text)
        .then(function () {
            alert("MFA key has been copied to the clipboard.");
        })
        .catch(function (error) {
            alert(error);
        });
}