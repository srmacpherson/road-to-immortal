import { useEffect, useState } from "react";
import { getDashboard, type Dashboard } from "./api/dashboardApi";

const STEAM_ID = "76561199124533567";

function App() {
    const [dashboard, setDashboard] = useState<Dashboard | null>(null);
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
            < p > Track the climb.Understand the game.Reach Immortal.</p>
                </div>

                < div className = "mmr-badge" >
                    <span>Current MMR </span>
                        < strong > { dashboard.mmr.current ?? "—" } </strong>
                        </div>
                        </header>

                        < main className = "dashboard" >
                            <section className="card" >
                                <h2>MMR Progression </h2>

                                    < div className = "chart-placeholder" >
                                        MMR chart coming next
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