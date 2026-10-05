import { useEffect, useState } from "react";
import { getDashboard, syncMatches, confirmMmr, updatePlayerMmr, type Dashboard } from "./api/dashboardApi";
import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer,
} from "recharts";

const STEAM_ID = "76561199124533567";

function App() {
    const [dashboard, setDashboard] = useState<Dashboard | null>(null);
    const [syncing, setSyncing] = useState(false);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [confirming, setConfirming] = useState(false);
    const [confirmValue, setConfirmValue] = useState<number | null>(null);
    const [manualMmr, setManualMmr] = useState<number | null>(null);
    const [updatingMmr, setUpdatingMmr] = useState(false);

    useEffect(() => {
        async function loadDashboard() {
            try {
                const data = await getDashboard(STEAM_ID);
                setDashboard(data);
            } catch (err) {
                setError("Failed to load dashboard.");
                console.error(err);
            } finally {
                setLoading(false);
            }
        }

        loadDashboard();
    }, []);

    async function handleSync() {
        if (!dashboard?.mmr.current) {
            return;
        }

        try {
            setSyncing(true);

            await syncMatches(STEAM_ID, dashboard.mmr.current);

            const refreshedDashboard = await getDashboard(STEAM_ID);
            setDashboard(refreshedDashboard);
        } catch (err) {
            console.error(err);
            setError("Failed to sync matches.");
        } finally {
            setSyncing(false);
        }
    }

    if (loading) {
        return <div className="app" > Loading dashboard...</div>;
    }

    if (error || !dashboard) {
        return <div className="app" > { error ?? "No dashboard data."
    } </div>;
}

return (
    <div className= "app" >
        <header className="header" >
            <div>
                <h1>Road to Immortal</h1>
                <p>Track the climb. Understand the game. Reach Immortal.</p>
            </div>

            <div className="mmr-badge">
                <span>Current MMR</span>
                <strong>{dashboard.mmr.current ?? "—"}</strong>
                {dashboard.mmr.predictedMmr != null && (
                    <div className="predicted">
                        <small>Predicted: </small>
                        <strong>{dashboard.mmr.predictedMmr}</strong>
                        <small> ({dashboard.mmr.predictionDelta >= 0 ? "+" : ""}{dashboard.mmr.predictionDelta})</small>
                    </div>
                )}
                <div className="manual-update">
                    <input
                        type="number"
                        placeholder="Set MMR"
                        value={manualMmr ?? ""}
                        onChange={(e) => setManualMmr(e.target.value ? Number(e.target.value) : null)}
                    />
                    <button
                        onClick={async () => {
                            if (manualMmr == null) return;
                            try {
                                setUpdatingMmr(true);
                                await updatePlayerMmr(STEAM_ID, manualMmr);
                                const refreshed = await getDashboard(STEAM_ID);
                                setDashboard(refreshed);
                            } catch (err) {
                                console.error(err);
                                setError("Failed to update MMR.");
                            } finally {
                                setUpdatingMmr(false);
                            }
                        }}
                        disabled={updatingMmr}
                    >
                        {updatingMmr ? "Updating..." : "Update MMR"}
                    </button>
                </div>
            </div>

            <button className="sync-button" onClick={handleSync} disabled={syncing}>
                {syncing ? "Syncing..." : "Sync Matches"}
            </button>

            {/* Confirmation prompt */}
            {dashboard.mmr.predictionNeedsConfirmation && (
                <div className="confirm-mmr">
                    <p>
                        Predicted MMR differs from confirmed by {dashboard.mmr.predictionDelta}. Please confirm your current MMR.
                    </p>
                    <div className="confirm-controls">
                        <input
                            type="number"
                            value={confirmValue ?? dashboard.mmr.predictedMmr ?? undefined}
                            onChange={(e) => setConfirmValue(Number(e.target.value))}
                        />
                        <button
                            onClick={async () => {
                                if (confirmValue == null) return;
                                try {
                                    setConfirming(true);
                                    await confirmMmr(STEAM_ID, confirmValue);
                                    const refreshed = await getDashboard(STEAM_ID);
                                    setDashboard(refreshed);
                                } catch (err) {
                                    console.error(err);
                                    setError("Failed to confirm MMR.");
                                } finally {
                                    setConfirming(false);
                                }
                            }}
                            disabled={confirming}
                        >
                            {confirming ? "Confirming..." : "Confirm MMR"}
                        </button>
                    </div>
                </div>
            )}
        </header>

        < main className = "dashboard" >
            <section className="card" >
                <h2>MMR Progression </h2>

                < div className = "chart-container" >
                    <ResponsiveContainer width="100%" height = { 300} >
    <LineChart
  data={ dashboard.mmr.history }
margin = {{ top: 10, right: 20, left: 0, bottom: 10 }}
>
    <CartesianGrid strokeDasharray="3 3" />

        <XAxis
    dataKey="recordedAt"
tickFormatter = {(value) =>
new Date(value).toLocaleDateString("en-GB", {
    day: "numeric",
    month: "short",
})
    }
  />

    < YAxis
domain = { ["dataMin - 100", "dataMax + 100"]}
tickFormatter = {(value) => `${value}`}
  />

    < Tooltip
labelFormatter = {(value) =>
new Date(value).toLocaleString("en-GB", {
    day: "numeric",
    month: "short",
    hour: "2-digit",
    minute: "2-digit",
})
    }
formatter = {(value) => [`${value} MMR`, "MMR"]}
  />

    < Line
type = "monotone"
dataKey = "mmr"
stroke = "#646cff"
strokeWidth = { 3}
dot = {{ r: 5 }}
activeDot = {{ r: 7 }}
  />
    </LineChart>
    </ResponsiveContainer>
    </div>
                                            </section>

                                            < section className = "grid" >
    <div className="card" >
        <h2>Recent Form </h2>

            < div className = "match-list" >
            {
                dashboard.recentForm.matches.map((match) => (
                    <div
                key= { match.matchId }
                className = {`match-card ${match.result === "W" ? "match-win" : "match-loss"
                        }`}
                >
                <div className="match-result" >
                    <strong>
                    { match.result === "W" ? "WIN" : "LOSS" }
                    </strong>

                    <span>
{
    new Date(match.matchDate).toLocaleDateString(
        "en-GB",
        {
            day: "numeric",
            month: "short",
        }
    )
}
</span>
    </div>

    < div className = "match-hero" >
        <span>Hero</span>
        < strong > { match.heroName } </strong>
        </div>

        < div className = "match-kda" >
            <span>K / D / A </span>
            <strong>
{ match.kills } / {match.deaths} / { match.assists }
</strong>
    </div>

    < div className = "match-duration" >
        <span>Duration </span>
        <strong>
{ Math.floor(match.duration / 60) }:
{
    (match.duration % 60)
    .toString()
    .padStart(2, "0")
}
</strong>
    </div>
    </div>
        ))}
</div>
    </div>

                                                            < div className = "card" >
                                                                <h2>Overall Stats </h2>

                                                                    < div className = "stats" >
                                                                        <div>
                                                                        <span>Win Rate </span>
                                                                            < strong > { dashboard.overall.winRate } % </strong>
                                                                            </div>

                                                                            < div >
                                                                            <span>Kills </span>
                                                                            < strong > { dashboard.overall.averageKills } </strong>
                                                                            </div>

                                                                            < div >
                                                                            <span>Deaths </span>
                                                                            < strong > { dashboard.overall.averageDeaths } </strong>
                                                                            </div>

                                                                            < div >
                                                                            <span>Assists </span>
                                                                            < strong > { dashboard.overall.averageAssists } </strong>
                                                                            </div>
                                                                            </div>
                                                                            </div>
                                                                            </section>

    < section className = "card" >
        <h2>Performance Snapshot </h2>

            < div className = "snapshot-grid" >
                <div className="snapshot-item" >
                    <span>Win Rate </span>
                        < strong > { dashboard.overall.winRate } % </strong>
                        </div>

                        < div className = "snapshot-item" >
                            <span>Recent Win Rate </span>
                                < strong > { dashboard.recentForm.winRate } % </strong>
                                </div>

                                < div className = "snapshot-item" >
                                    <span>Average KDA </span>
                                        <strong>
{ dashboard.overall.averageKills } /{" "}
{ dashboard.overall.averageDeaths } /{" "}
{ dashboard.overall.averageAssists }
</strong>
    </div>

    < div className = "snapshot-item" >
        <span>MMR Gained </span>
            <strong>
{ dashboard.mmr.gained >= 0 ? "+" : "" }
{ dashboard.mmr.gained }
</strong>
    </div>

    < div className = "snapshot-item" >
        <span>Highest MMR </span>
            < strong > { dashboard.mmr.highest ?? "—" } </strong>
            </div>
            </div>
            </section>

    < section className = "card" >
        <h2>Recent Performance </h2>

            < div className = "trend-card" >
                <div>
                <span>Overall Win Rate </span>
                    < strong > { dashboard.overall.winRate } % </strong>
                    </div>

                    < div >
                    <span>Recent Win Rate </span>
                        < strong > { dashboard.recentForm.winRate } % </strong>
                        </div>

                        < div >
                        <span>Recent Games </span>
                            < strong > { dashboard.recentForm.games } </strong>
                            </div>
                            </div>

                            < div className = "trend-stats" >
                                <div className="trend-stat" >
                                    <span>Overall Kills </span>
                                        < strong > { dashboard.overall.averageKills } </strong>
                                        </div>

                                        < div className = "trend-stat" >
                                            <span>Overall Deaths </span>
                                                < strong > { dashboard.overall.averageDeaths } </strong>
                                                </div>

                                                < div className = "trend-stat" >
                                                    <span>Overall Assists </span>
                                                        < strong > { dashboard.overall.averageAssists } </strong>
                                                        </div>

                                                        < div className = "trend-stat" >
                                                            <span>Recent Kills </span>
                                                                <strong>
{
    dashboard.recentForm.games > 0
    ? (
        dashboard.recentForm.matches.reduce(
            (total, match) => total + match.kills,
            0
        ) / dashboard.recentForm.games
    ).toFixed(2)
    : "—"
}
</strong>
    </div>

    < div className = "trend-stat" >
        <span>Recent Deaths </span>
            <strong>
{
    dashboard.recentForm.games > 0
    ? (
        dashboard.recentForm.matches.reduce(
            (total, match) => total + match.deaths,
            0
        ) / dashboard.recentForm.games
    ).toFixed(2)
    : "—"
}
</strong>
    </div>

    < div className = "trend-stat" >
        <span>Recent Assists </span>
            <strong>
{
    dashboard.recentForm.games > 0
    ? (
        dashboard.recentForm.matches.reduce(
            (total, match) => total + match.assists,
            0
        ) / dashboard.recentForm.games
    ).toFixed(2)
    : "—"
}
</strong>
    </div>
    </div>
    </section>

    < section className = "card" >
        <h2>Hero Performance </h2>

            < div className = "hero-list" >
            {
                dashboard.heroes.map((hero) => (
                    <div className= "hero-performance" key = { hero.heroId } >
                    <div className="hero-info" >
                    <strong>{ hero.heroName } </strong>
                    < span > { hero.games } games </span>
                    </div>

                < div className = "hero-record" >
                <span>
                { hero.wins }W - { hero.losses }L
                </span>
                < strong > { hero.winRate } % WR </strong>
                </div>

                < div className = "hero-kda" >
                <span>K / D / A </span>
                <strong>
                        { hero.averageKills } / { hero.averageDeaths } / { " "}
                        { hero.averageAssists }
                    </strong>
                    </div>
                    </div>
                ))
            }
                </div>
                </section>
    </main>
    </div>
  );
}

export default App;