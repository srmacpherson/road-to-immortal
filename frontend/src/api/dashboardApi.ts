export interface Dashboard {
    steamId: number;

    mmr: {
        starting: number | null;
        current: number | null;
        highest: number | null;
        gained: number;
        history: {
            mmr: number;
            recordedAt: string;
        }[];
        confirmedMmr?: number | null;
        predictedMmr?: number | null;
        predictionDelta?: number | null;
        predictionNeedsConfirmation?: boolean;
    };

    overall: {
        games: number;
        wins: number;
        losses: number;
        winRate: number;
        averageKills: number;
        averageDeaths: number;
        averageAssists: number;
    };

    heroes: {
        heroId: number;
        heroName: string;
        games: number;
        wins: number;
        losses: number;
        winRate: number;
        averageKills: number;
        averageDeaths: number;
        averageAssists: number;
    }[];

    recentForm: {
        games: number;
        wins: number;
        losses: number;
        winRate: number;
        matches: {
            matchId: number;
            result: string;
            heroId: number;
            heroName: string;
            kills: number;
            deaths: number;
            assists: number;
            duration: number;
            matchDate: string;
        }[];
    };
}

export async function updatePlayerMmr(
    steamId: string,
    currentMmr: number
): Promise<void> {
    const response = await fetch(
        `${API_BASE_URL}/players/${steamId}`,
        {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ currentMmr }),
        }
    );

    if (!response.ok) {
        throw new Error("Failed to update player MMR");
    }
}

export async function confirmMmr(
    steamId: string,
    mmr: number
): Promise<void> {
    const response = await fetch(
        `${API_BASE_URL}/players/${steamId}/mmr/confirm`,
        {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ mmr }),
        }
    );

    if (!response.ok) {
        throw new Error("Failed to confirm MMR");
    }
}

const API_BASE_URL = "http://127.0.0.1:5184";

export async function getDashboard(
    steamId: string
): Promise<Dashboard> {
    const response = await fetch(
        `${API_BASE_URL}/players/${steamId}/dashboard`
    );

    if (!response.ok) {
        throw new Error("Failed to fetch dashboard");
    }

    return response.json();
}

export async function syncMatches(
    steamId: string,
    mmr: number
): Promise<void> {
    const response = await fetch(
        `${API_BASE_URL}/players/${steamId}/matches/sync?mmr=${mmr}`,
        {
            method: "POST",
        }
    );

    if (!response.ok) {
        throw new Error("Failed to sync matches");
    }
}