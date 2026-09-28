function App() {
    return (
        <div className= "app" >
        <header className="header" >
            <div>
            <h1>Road to Immortal </h1>
                < p > Track the climb.Understand the game.Reach Immortal.</p>
                    </div>

                    < div className = "mmr-badge" >
                        <span>Current MMR </span>
                            < strong > 5, 360 </strong>
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
                                                                <span className="win" > W </span>
                                                                    < span className = "win" > W </span>
                                                                        < span className = "loss" > L </span>
                                                                            < span className = "win" > W </span>
                                                                                < span className = "win" > W </span>
                                                                                    < span className = "loss" > L </span>
                                                                                        < span className = "win" > W </span>
                                                                                            < span className = "win" > W </span>
                                                                                                < span className = "win" > W </span>
                                                                                                    < span className = "loss" > L </span>
                                                                                                        </div>
                                                                                                        </div>

                                                                                                        < div className = "card" >
                                                                                                            <h2>Overall Stats </h2>

                                                                                                                < div className = "stats" >
                                                                                                                    <div>
                                                                                                                    <span>Win Rate </span>
                                                                                                                        < strong > 60 % </strong>
                                                                                                                        </div>

                                                                                                                        < div >
                                                                                                                        <span>Kills </span>
                                                                                                                        < strong > 8.4 </strong>
                                                                                                                        </div>

                                                                                                                        < div >
                                                                                                                        <span>Deaths </span>
                                                                                                                        < strong > 6.2 </strong>
                                                                                                                        </div>

                                                                                                                        < div >
                                                                                                                        <span>Assists </span>
                                                                                                                        < strong > 14.1 </strong>
                                                                                                                        </div>
                                                                                                                        </div>
                                                                                                                        </div>
                                                                                                                        </section>

                                                                                                                        < section className = "card" >
                                                                                                                            <h2>Hero Performance </h2>

                                                                                                                                < div className = "hero-row" >
                                                                                                                                    <strong>Sniper </strong>
                                                                                                                                    < span > 5 games </span>
                                                                                                                                        < span > 60 % WR </span>
                                                                                                                                        </div>

                                                                                                                                        < div className = "hero-row" >
                                                                                                                                            <strong>Axe </strong>
                                                                                                                                            < span > 4 games </span>
                                                                                                                                                < span > 75 % WR </span>
                                                                                                                                                </div>

                                                                                                                                                < div className = "hero-row" >
                                                                                                                                                    <strong>Puck </strong>
                                                                                                                                                    < span > 3 games </span>
                                                                                                                                                        < span > 33 % WR </span>
                                                                                                                                                        </div>
                                                                                                                                                        </section>
                                                                                                                                                        </main>
                                                                                                                                                        </div>
  );
}

export default App;