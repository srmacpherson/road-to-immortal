import { useEffect, useState } from "react";
import { getDashboard, syncMatches, type Dashboard } from "./api/dashboardApi";
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
                <h1>Road to Immortal </h1>
                <p> Track the climb.Understand the game.Reach Immortal.</p>
            </div>

            < div className = "mmr-badge" >
                <span>Current MMR </span>
                <strong> { dashboard.mmr.current ?? "—" } </strong>
                    </div>
    < button
className = "sync-button"
onClick = { handleSync }
disabled = { syncing }
    >
{ syncing? "Syncing...": "Sync Matches" }
    </button>
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

                                                        < div className = "form" >
                                                        {
                                                            dashboard.recentForm.matches.map((match) => (
                                                                <span
                  key= { match.matchId }
                  className = { match.result === "W" ? "win" : "loss" }
                                                                >
                                                                { match.result }
                                                                </span>
                                                            ))
                                                        }
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
                                                                                <h2>Hero Performance </h2>

{
    dashboard.heroes.map((hero) => (
        <div className= "hero-row" key = { hero.heroId } >
        <strong>{ hero.heroName } </strong>
        < span > { hero.games } games </span>
        < span > { hero.winRate } % WR </span>
    </div>
    ))
}
</section>
    </main>
    </div>
  );
}

export default App;