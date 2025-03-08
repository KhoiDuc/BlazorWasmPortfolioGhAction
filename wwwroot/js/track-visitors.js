console.log(window.location.hostname);
if (//window.location.hostname !== '127.0.0.1' &&
    //window.location.hostname !== 'localhost' &&
    window.location.hostname !== 'khoiduc.github.io') {
    console.log("skip record");
} else {
    const firebaseConfig = {
        apiKey: "AIzaSyAAOzL2OpC1CRla4V5-aBMfAXczYuHx8L4",
        authDomain: "my-cv-project-40d66.firebaseapp.com",
        projectId: "my-cv-project-40d66",
        storageBucket: "my-cv-project-40d66.firebasestorage.app",
        messagingSenderId: "1093313907484",
        appId: "1:1093313907484:web:214bd30e300994f20c88d9",
        measurementId: "G-QGDR3K5CCX"
    };

    firebase.initializeApp(firebaseConfig);
    const db = firebase.firestore();
    console.log("Firebase initialized:", firebase.apps.length > 0);
    console.log("Firestore instance:", !!db);
    fetch('https://ipinfo.io/json')
        .then(response => response.json())
        .then(data => {
            const ip = data.ip || '';
            const city = data.city || '';
            const region = data.region || '';
            const country = data.country || '';

            const now = new Date();
            const utcTimestamp = now.getTime() + (now.getTimezoneOffset() * 60000);
            const vietnamOffset = 7 * 60 * 60000;
            const vietnamTime = new Date(utcTimestamp + vietnamOffset);

            const year = vietnamTime.getFullYear();
            const month = String(vietnamTime.getMonth() + 1).padStart(2, '0');
            const day = String(vietnamTime.getDate()).padStart(2, '0');
            const hours = String(vietnamTime.getHours()).padStart(2, '0');
            const minutes = String(vietnamTime.getMinutes()).padStart(2, '0');
            const seconds = String(vietnamTime.getSeconds()).padStart(2, '0');
            const milliseconds = String(vietnamTime.getMilliseconds()).padStart(3, '0');

            const timestamp = `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;

            const log = {
                ip: ip,
                city: city,
                region: region,
                country: country,
                timestamp: timestamp
            };

            const logPath = "visitor_logs";
            const docId = `${year}${month}${day}${hours}${minutes}${seconds}${milliseconds}`;
            console.log("Writing to:", logPath, "with doc ID:", docId, "data:", log);
            db.collection(logPath).doc(docId).set(log)
                .then(() => {
                    console.log("Visitor data saved successfully");
                })
                .catch(error => {
                    console.error("Error saving visitor data: ", error);
                });
        })
        .catch(error => {
            console.error("Error fetching IP info: ", error);
        });
}