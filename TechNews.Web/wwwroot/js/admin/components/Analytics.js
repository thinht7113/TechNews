const { ref, onMounted, onUnmounted } = Vue;

export default {
    setup() {
        const overview = ref(null);
        const isLoading = ref(false);
        const days = ref(30);
        let chartInstance = null;

        const fetchOverview = async () => {
            isLoading.value = true;
            try {
                const res = await fetch(`/api/analytics/overview?days=${days.value}`);
                if (res.ok) {
                    overview.value = await res.json();
                    renderChart();
                }
            } catch (e) { console.error(e); }
            finally { isLoading.value = false; }
        };

        const renderChart = () => {
            if (!overview.value || !overview.value.dailyViews) return;
            const ctx = document.getElementById('viewsChart');
            if (!ctx) return;
            if (chartInstance) chartInstance.destroy();

            chartInstance = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: overview.value.dailyViews.map(d => d.date),
                    datasets: [{
                        label: 'LÆ°á»£t xem',
                        data: overview.value.dailyViews.map(d => d.views),
                        borderColor: '#2563eb',
                        backgroundColor: 'rgba(159, 34, 78, 0.1)',
                        fill: true,
                        tension: 0.4,
                        pointRadius: 2,
                        pointHoverRadius: 5
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: {
                        y: { beginAtZero: true, grid: { color: '#f1f5f9' } },
                        x: { grid: { display: false }, ticks: { maxTicksLimit: 10 } }
                    }
                }
            });
        };

        onMounted(fetchOverview);
        onUnmounted(() => { if (chartInstance) chartInstance.destroy(); });

        return { overview, isLoading, days, fetchOverview };
    },
    template: `
        <div>
            <div class="mb-6 flex items-center justify-between">
                <div>
                    <h2 class="text-2xl font-bold text-black">Thá»‘ng kÃª ná»™i dung</h2>
                    <p class="text-sm text-slate-500 mt-1">PhÃ¢n tÃ­ch lÆ°á»£t xem vÃ  hÃ nh vi Ä‘á»c giáº£</p>
                </div>
                <select v-model="days" @change="fetchOverview" class="border border-stroke rounded-lg px-4 py-2 text-sm bg-white">
                    <option :value="7">7 ngÃ y</option>
                    <option :value="14">14 ngÃ y</option>
                    <option :value="30">30 ngÃ y</option>
                    <option :value="90">90 ngÃ y</option>
                </select>
            </div>

            <div v-if="isLoading" class="text-center py-10 text-slate-500">
                <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin mx-auto mb-2"></div>
                Äang táº£i...
            </div>

            <template v-else-if="overview">
                <!-- Stat Cards -->
                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                    <div class="bg-white rounded-lg border border-stroke p-5">
                        <div class="flex items-center justify-between">
                            <div>
                                <p class="text-sm text-slate-500">Tá»•ng lÆ°á»£t xem</p>
                                <h3 class="text-2xl font-bold text-black mt-1">{{ overview.totalViews?.toLocaleString() || 0 }}</h3>
                            </div>
                            <div class="w-12 h-12 bg-primary/10 rounded-full flex items-center justify-center">
                                <i class="bi bi-eye text-xl text-primary"></i>
                            </div>
                        </div>
                    </div>
                    <div class="bg-white rounded-lg border border-stroke p-5">
                        <div class="flex items-center justify-between">
                            <div>
                                <p class="text-sm text-slate-500">KhÃ¡ch truy cáº­p</p>
                                <h3 class="text-2xl font-bold text-black mt-1">{{ overview.uniqueVisitors?.toLocaleString() || 0 }}</h3>
                            </div>
                            <div class="w-12 h-12 bg-blue-50 rounded-full flex items-center justify-center">
                                <i class="bi bi-people text-xl text-blue-600"></i>
                            </div>
                        </div>
                    </div>
                    <div class="bg-white rounded-lg border border-stroke p-5">
                        <div class="flex items-center justify-between">
                            <div>
                                <p class="text-sm text-slate-500">Thá»i gian Ä‘á»c TB</p>
                                <h3 class="text-2xl font-bold text-black mt-1">{{ overview.avgTimeOnPage || 0 }}s</h3>
                            </div>
                            <div class="w-12 h-12 bg-green-50 rounded-full flex items-center justify-center">
                                <i class="bi bi-clock text-xl text-green-600"></i>
                            </div>
                        </div>
                    </div>
                    <div class="bg-white rounded-lg border border-stroke p-5">
                        <div class="flex items-center justify-between">
                            <div>
                                <p class="text-sm text-slate-500">Tá»‰ lá»‡ thoÃ¡t</p>
                                <h3 class="text-2xl font-bold text-black mt-1">{{ overview.bounceRate || 0 }}%</h3>
                            </div>
                            <div class="w-12 h-12 bg-amber-50 rounded-full flex items-center justify-center">
                                <i class="bi bi-box-arrow-right text-xl text-amber-600"></i>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Chart -->
                <div class="bg-white rounded-lg border border-stroke p-5 mb-6">
                    <h4 class="font-bold text-black mb-4">LÆ°á»£t xem theo ngÃ y</h4>
                    <div style="height: 300px">
                        <canvas id="viewsChart"></canvas>
                    </div>
                </div>

                <!-- Tables -->
                <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    <!-- Top Posts -->
                    <div class="bg-white rounded-lg border border-stroke p-5">
                        <h4 class="font-bold text-black mb-4">BÃ i viáº¿t ná»•i báº­t</h4>
                        <div v-if="overview.topPosts && overview.topPosts.length > 0" class="space-y-3">
                            <div v-for="(post, i) in overview.topPosts" :key="post.postId"
                                class="flex items-center justify-between text-sm border-b border-stroke pb-2 last:border-0">
                                <div class="flex items-center gap-3">
                                    <span class="w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold"
                                        :class="i < 3 ? 'bg-primary text-white' : 'bg-gray-100 text-gray-600'">{{ i + 1 }}</span>
                                    <span class="text-black">Post #{{ post.postId }}</span>
                                </div>
                                <div class="flex items-center gap-4 text-xs text-slate-500">
                                    <span><i class="bi bi-eye"></i> {{ post.views }}</span>
                                    <span><i class="bi bi-clock"></i> {{ post.avgTime }}s</span>
                                </div>
                            </div>
                        </div>
                        <p v-else class="text-sm text-slate-500 text-center py-4">ChÆ°a cÃ³ dá»¯ liá»‡u</p>
                    </div>

                    <!-- Top Referrers -->
                    <div class="bg-white rounded-lg border border-stroke p-5">
                        <h4 class="font-bold text-black mb-4">Nguá»“n truy cáº­p</h4>
                        <div v-if="overview.topReferrers && overview.topReferrers.length > 0" class="space-y-3">
                            <div v-for="ref in overview.topReferrers" :key="ref.source"
                                class="flex items-center justify-between text-sm border-b border-stroke pb-2 last:border-0">
                                <span class="text-black truncate max-w-[70%]">{{ ref.source }}</span>
                                <span class="text-xs text-slate-500 font-medium">{{ ref.count }} lÆ°á»£t</span>
                            </div>
                        </div>
                        <p v-else class="text-sm text-slate-500 text-center py-4">ChÆ°a cÃ³ dá»¯ liá»‡u</p>
                    </div>
                </div>
            </template>

            <div v-else class="text-center py-20 bg-white rounded-lg border border-stroke">
                <i class="bi bi-bar-chart text-5xl text-slate-300 mb-4 block"></i>
                <p class="text-lg font-medium text-black">ChÆ°a cÃ³ dá»¯ liá»‡u thá»‘ng kÃª</p>
                <p class="text-sm text-slate-500 mt-1">Dá»¯ liá»‡u sáº½ xuáº¥t hiá»‡n khi cÃ³ lÆ°á»£t truy cáº­p</p>
            </div>
        </div>
    `
};

