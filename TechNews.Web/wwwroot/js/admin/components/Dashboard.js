const { ref, onMounted } = Vue;

export default {
    setup() {
        const stats = ref({
            totalPosts: 0,
            totalCategories: 0,
            draftPosts: 0,
            mediaCount: 0,
            chart: { labels: [], data: [] },
            topPosts: [],
            recentUsers: []
        });

        let chartInstance = null;

        const renderChart = () => {
            const ctx = document.getElementById('analyticsChart');
            if (!ctx) return;

            if (chartInstance) chartInstance.destroy();

            chartInstance = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: stats.value.chart.labels,
                    datasets: [{
                        label: 'Bài viết mới',
                        data: stats.value.chart.data,
                        borderColor: '#2563eb',
                        backgroundColor: 'rgba(159, 34, 78, 0.1)',
                        borderWidth: 2,
                        fill: true,
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: false }
                    },
                    scales: {
                        y: { beginAtZero: true, ticks: { stepSize: 1 } }
                    }
                }
            });
        };

        onMounted(async () => {
            try {
                const res = await fetch('/api/dashboard/stats');
                if (res.ok) {
                    stats.value = await res.json();
                    setTimeout(renderChart, 100);
                }
            } catch (e) { console.error(e); }
        });

        return { stats };
    },
    template: `
        <div>
           <!-- Summary Cards -->
           <div class="grid grid-cols-1 gap-4 md:grid-cols-2 md:gap-6 xl:grid-cols-4 2xl:gap-7.5 mb-6">
                 <div class="rounded-sm border border-stroke bg-white px-7.5 py-6 shadow-sm">
                    <div class="flex h-11.5 w-11.5 items-center justify-center rounded-full bg-blue-50 text-secondary">
                        <i class="bi bi-file-text text-xl"></i>
                    </div>
                    <div class="mt-4">
                         <h4 class="text-2xl font-bold text-black">{{ stats.totalPosts }}</h4>
                        <span class="text-sm font-medium text-slate-500">Tổng bài viết</span>
                    </div>
                </div>
                 <div class="rounded-sm border border-stroke bg-white px-7.5 py-6 shadow-sm">
                    <div class="flex h-11.5 w-11.5 items-center justify-center rounded-full bg-blue-50 text-white bg-primary">
                        <i class="bi bi-folder text-xl"></i>
                    </div>
                    <div class="mt-4">
                         <h4 class="text-2xl font-bold text-black">{{ stats.totalCategories }}</h4>
                        <span class="text-sm font-medium text-slate-500">Chuyên mục</span>
                    </div>
                </div>
                 <div class="rounded-sm border border-stroke bg-white px-7.5 py-6 shadow-sm">
                    <div class="flex h-11.5 w-11.5 items-center justify-center rounded-full bg-yellow-50 text-yellow-600">
                         <i class="bi bi-pencil-square text-xl"></i>
                    </div>
                    <div class="mt-4">
                         <h4 class="text-2xl font-bold text-black">{{ stats.draftPosts }}</h4>
                        <span class="text-sm font-medium text-slate-500">Bản nháp</span>
                    </div>
                </div>
                 <div class="rounded-sm border border-stroke bg-white px-7.5 py-6 shadow-sm">
                    <div class="flex h-11.5 w-11.5 items-center justify-center rounded-full bg-green-50 text-green-600">
                         <i class="bi bi-images text-xl"></i>
                    </div>
                    <div class="mt-4">
                         <h4 class="text-2xl font-bold text-black">{{ stats.mediaCount }}</h4>
                        <span class="text-sm font-medium text-slate-500">Files Media</span>
                    </div>
                </div>
            </div>

            <!-- Charts & Tables -->
             <div class="grid grid-cols-1 gap-4 md:grid-cols-12 mb-4">
                 <!-- Main Chart -->
                 <div class="md:col-span-8 rounded-sm border border-stroke bg-white shadow-default p-6">
                    <h3 class="font-bold text-xl mb-4 text-black">Thống kê bài viết mới (7 ngày qua)</h3>
                    <div class="relative h-64 w-full">
                        <canvas id="analyticsChart"></canvas>
                    </div>
                 </div>
                 
                 <!-- Top Posts -->
                 <div class="md:col-span-4 rounded-sm border border-stroke bg-white shadow-default p-6">
                    <h3 class="font-bold text-xl mb-4 text-black">Bài viết xem nhiều</h3>
                    <ul class="space-y-3">
                        <li v-for="(post, idx) in stats.topPosts" :key="idx" class="flex justify-between items-center text-sm border-b border-stroke pb-2 last:border-0">
                            <a :href="'/post/'+post.slug" target="_blank" class="font-medium text-black truncate pr-2 hover:text-primary flex-1">{{ post.title }}</a>
                            <span class="text-slate-500 text-xs shrink-0 bg-gray-100 px-2 py-1 rounded">{{ post.viewCount }} view</span>
                        </li>
                    </ul>
                 </div>
             </div>

             <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
                 <!-- Recent Users -->
                 <div class="rounded-sm border border-stroke bg-white shadow-default p-6">
                    <h3 class="font-bold text-xl mb-4 text-black">Thành viên mới</h3>
                    <ul class="space-y-3">
                        <li v-for="(user, idx) in stats.recentUsers" :key="idx" class="flex justify-between text-sm border-b border-stroke pb-2 last:border-0">
                            <div class="flex items-center gap-3">
                                <div class="h-8 w-8 rounded-full bg-gray-200 flex items-center justify-center text-xs font-bold">{{ user.fullName.charAt(0) }}</div>
                                <div>
                                    <p class="font-medium text-black">{{ user.fullName }}</p>
                                    <p class="text-xs text-slate-500">{{ user.email }}</p>
                                </div>
                            </div>
                            <span class="text-slate-500 text-xs shrink-0 self-center">{{ user.joined }}</span>
                        </li>
                    </ul>
                 </div>
                 
                 <div class="rounded-sm border border-stroke bg-white shadow-default p-6 flex items-center justify-center flex-col text-center">
                    <div class="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mb-4">
                        <i class="bi bi-shield-check text-3xl text-primary"></i>
                    </div>
                    <h3 class="font-bold text-lg mb-2">Hệ thống vận hành tốt</h3>
                    <p class="text-sm text-slate-500">Database Connection: OK</p>
                    <p class="text-sm text-slate-500">Disk Usage: Normal</p>
                 </div>
             </div>
        </div>
    `
};
