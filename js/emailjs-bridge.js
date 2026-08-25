window.emailJsBridge = {
    send: async function (publicKey, serviceId, templateId, templateParams) {
        if (!window.emailjs) {
            await new Promise(function (resolve, reject) {
                var script = document.createElement('script');
                script.src = 'https://cdn.jsdelivr.net/npm/@emailjs/browser@4/dist/email.min.js';
                script.onload = resolve;
                script.onerror = reject;
                document.head.appendChild(script);
            });
            emailjs.init(publicKey);
        }

        return emailjs.send(serviceId, templateId, templateParams, publicKey);
    }
};
