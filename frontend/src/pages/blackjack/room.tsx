import { HubConnectionBuilder } from "@microsoft/signalr";
import { useEffect, useMemo } from "react";
import { useParams } from "react-router";

export function Room() {
    const params = useParams();
    const roomId = useMemo(
        () => {
            console.log("params", params);
            return params.roomId || crypto.randomUUID()
        },
        [params]
    );

    useEffect(() => {
        console.log("roomId", roomId);
        if (!roomId) return;
        const playerId =
            localStorage.getItem("playerId") || crypto.randomUUID();

        localStorage.setItem("playerId", playerId);

        const connection = new HubConnectionBuilder()
            .withUrl(
                `http://localhost:5106/blackjack?playerId=${playerId}&roomId=${roomId}`
            )
            .build();

        connection.on("Welcome", (msg) => console.log(msg));
        // connection.on("PlayerConnected", (pid) => console.log(`Jugador conectado: ${pid}`));
        // connection.on("ReceiveMove", (pid, move) => console.log(`${pid} hizo ${move}`));

        connection.start().then(() => {
            console.log("Conectado a la sala");
        });
    }, [roomId]);

    return(
        <>a</>
    )
}
