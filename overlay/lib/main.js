const { createApp, ref, defineAsyncComponent, watch } = Vue;
// import DigitRoll from '@huoyu/vue-digitroll';
const { DigitAnimationGroup } = VueDigitAnimation;

// Module-level WebSocket reference
let ws = null;

// Create a reactive state object using ref
const initialState = {
    // Game state
    isInGame: false,
    isMusicSelected: false,
    isInResult: false,
    musicId: 0,
    isCurrentVersion: false,
    musicTitle: 'N/A',
    musicLevel: 0,
    playCount: 0,
    retryCount: 0,
    musicBPM: 0,
    musicArtist: 'N/A',
    difficulty: 'NONE',
    designer: 'N/A',
    b50: 0.0,
    n10: 0.0,
    p50: 0.0,
    n10_raw: 0.0,
    b50_delta: 0.0,
    n10_delta: 0.0,
    p50_delta: 0.0,
    n10_raw_delta: 0.0,

    // Judge counts
    judgeCrBreak: { count: 0, early: 0, late: 0 },
    judgeBreak: { count: 0, early: 0, late: 0 },
    judgeHit: { count: 0, early: 0, late: 0 },
    judgeMiss: 0,
    
    // Best records
    best: {
        bestRank: 0,
        platinumRank: 0,
        techScore: 0,
        platinumScore: 0,
        platinumStar: 0,
        isAllBreakPlus: false,
        isAllBreak: false,
        isFullCombo: false,
        isFullBell: false,
        rating: 0,
        platinumRating: 0,
        toNextPlatinum: 0,
        fromLastPlatinum: 0,
        technicalRank: ""
    },
    
    // Current performance
    platinumScoreMax: 0,
    platinumScoreLost: 0,
    current: {
        bellLost: 0,
        totalBell: 0,
        totalNote: 0,
        scoreNote: 0,
        scoreBell: 0,
        techScore: 0,
        platinumScore: 0,
        platinumStar: 0,
        isAllBreakPlus: false,
        isAllBreak: false,
        isFullCombo: false,
        isFullBell: false,
        rating: 0,
        damage: 0,
        platinumRating: 0,
        toNextPlatinum: 0,
        fromLastPlatinum: 0,
        technicalRank: ""
    }
};

const options = {
    moduleCache: {
    vue: Vue
    },
    async getFile(url) {
    
    const res = await fetch(url);
    if ( !res.ok )
        throw Object.assign(new Error(res.statusText + ' ' + url), { res });
    return {
        getContentData: asBinary => asBinary ? res.arrayBuffer() : res.text(),
    }
    },
    addStyle(textContent) {

    const style = Object.assign(document.createElement('style'), { textContent });
    const ref = document.head.getElementsByTagName('style')[0] || null;
    document.head.insertBefore(style, ref);
    },
}

console.log(SmartMarquee)

// Export the ref state

const app = createApp({
    components: {
        DigitAnimationGroup,
        SmartMarquee,
    },
    setup() {
        const isConnected = ref(false);
        const mu3 = ref(initialState);
        const showRatingDelta = ref(false);
        let toggleInterval = null;

        // Watch for changes to mu3.isInResult
        watch(() => mu3.value.isInResult, (isInResult) => {
            if (isInResult) {
                // Start toggling showRatingDelta every 5 seconds
                showRatingDelta.value = true;
                console.log('showRatingDelta', showRatingDelta.value);
                toggleInterval = setInterval(() => {
                    showRatingDelta.value = !showRatingDelta.value;
                    console.log('showRatingDelta', showRatingDelta.value);
                }, 5000);
            } else {
                // Stop toggling and reset showRatingDelta
                if (toggleInterval) {
                    clearInterval(toggleInterval);
                    toggleInterval = null;
                }
                showRatingDelta.value = false;
            }
        });

        const connectWebSocket = () => {
            ws = new ReconnectingWebSocket('ws://localhost:9715/state', null, {
                debug: false,
                reconnectInterval: 1000,
                maxReconnectInterval: 30000,
                reconnectDecay: 1.5,
                timeoutInterval: 2000,
                maxReconnectAttempts: null
            });
            
            ws.onopen = () => {
                console.log('Connected to WebSocket server');
                isConnected.value = true;
                // Send hello message
                ws.send('tsumugi on air');
            };

            ws.onclose = () => {
                console.log('Disconnected from WebSocket server');
                isConnected.value = false;
            };

            ws.onmessage = (event) => {
                try {
                    const data = JSON.parse(event.data);
                    mu3.value = data;
                } catch (e) {
                    console.error('Error parsing WebSocket message:', e);
                }
            };

            ws.onerror = (error) => {
                console.error('WebSocket error:', error);
            };
        };
        // Connect on mount
        connectWebSocket();

        const ordinal = (number) => {
            var b = number % 10;
            return number.toString() + (
                (~~(number % 100 / 10) === 1) ? 'th' :
                (b === 1) ? 'st' :
                (b === 2) ? 'nd' :
                (b === 3) ? 'rd' : 'th'
            );
        }

        return {
            mu3,
            ordinal,
            isConnected,
            showRatingDelta,
        };
    }
});

app.mount('#app'); 
