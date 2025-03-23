export async function SignIn(dotNetReference, loginModel) {
    try {
        const url = new URL(window.location.href);
        const params = new URLSearchParams(url.search);
        loginModel.ReturnUrl = params.get("ReturnUrl");

        const response = await fetch("/api/auth/login", {
            method: "POST",
            redirect: "manual",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json;charset=UTF-8'
            },
            body: JSON.stringify(loginModel)
        });

        const message = await response.text();
        const cleanedMessage = message.replace(/^"|"$/g, "");
        if (response.ok) {
            dotNetReference.invokeMethodAsync('HandleLoginSuccess', cleanedMessage);
        }
        else {
            dotNetReference.invokeMethodAsync('HandleLoginFailed', cleanedMessage);
        }
    }
    catch {
        dotNetReference.invokeMethodAsync('HandleLoginFailed', null);
    }
}