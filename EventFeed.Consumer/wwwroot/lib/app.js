const connection = new signalR.HubConnectionBuilder()
    .withUrl("/realtime/clicks")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection
    .on("Click", (clicks) => { document.getElementById('clicks').innerText = clicks; });
connection
    .start()
    .then(function () { console.log("connected"); });

async function start() {
    try {
        await connection.start();
        console.log("connected");
        connection.invoke("Refresh").catch(err => console.error(err.toString()));
    } catch (err) {
        console.log(err);
        setTimeout(() => start(), 5000);
    }
}

connection.onclose(async () => {
    console.log("lost connection");
    await start();
});
