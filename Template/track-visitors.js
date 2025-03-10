if (window.location.hostname !== 'siyuanli.tech' &&
    window.location.hostname !== 'www.siyuanli.tech' &&
    // window.location.hostname !== '127.0.0.1' &&
    window.location.hostname !== 'liaoyanqing666.github.io') {
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

    fetch('https://ipapi.co/json/')
        .then(response => response.json())
        .then(data => {
            const ip = data.ip || '';
            const city = data.city || '';
            const region = data.region || '';
            const country = data.country_name || '';

            //const excludedCountries = ["Japan", "Singapore", "Taiwan"];
            //if (excludedCountries.includes(country)) {
            //    console.log(`Visitor from ${country} is excluded from logging.`);
            //    return;
            //}

            // Convert current time to Vietnam Time (UTC+7)
            const now = new Date();
            const utcTimestamp = now.getTime() + (now.getTimezoneOffset() * 60000);
            const vietnamOffset = 7 * 60 * 60000; // Change from 8 to 7
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

            const logPath = `${year}/${month}/${day}`;
            db.collection(logPath).doc(`${year}${month}${day}${hours}${minutes}${seconds}${milliseconds}`).set(log)
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
