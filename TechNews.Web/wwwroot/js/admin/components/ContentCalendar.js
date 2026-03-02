const { ref, onMounted } = Vue;

export default {
    setup() {
        const events = ref([]);
        const currentMonth = ref(new Date());
        const isLoading = ref(false);

        const statusLabels = {
            'Draft': 'Nháp', 'Published': 'Đã phát hành', 'Scheduled': 'Đã lên lịch',
            'InReview': 'Đang duyệt', 'Rejected': 'Từ chối', 'Archived': 'Lưu trữ'
        };

        const daysInMonth = (date) => new Date(date.getFullYear(), date.getMonth() + 1, 0).getDate();
        const firstDayOfMonth = (date) => new Date(date.getFullYear(), date.getMonth(), 1).getDay();

        const monthName = () => {
            return currentMonth.value.toLocaleDateString('vi-VN', { month: 'long', year: 'numeric' });
        };

        const calendarDays = () => {
            const days = [];
            const totalDays = daysInMonth(currentMonth.value);
            const firstDay = firstDayOfMonth(currentMonth.value);
            // Padding
            for (let i = 0; i < firstDay; i++) days.push(null);
            for (let d = 1; d <= totalDays; d++) days.push(d);
            return days;
        };

        const getEventsForDay = (day) => {
            if (!day) return [];
            const dateStr = `${currentMonth.value.getFullYear()}-${String(currentMonth.value.getMonth() + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            return events.value.filter(e => e.start && e.start.startsWith(dateStr));
        };

        const isToday = (day) => {
            if (!day) return false;
            const today = new Date();
            return day === today.getDate() && currentMonth.value.getMonth() === today.getMonth() && currentMonth.value.getFullYear() === today.getFullYear();
        };

        const prevMonth = () => {
            currentMonth.value = new Date(currentMonth.value.getFullYear(), currentMonth.value.getMonth() - 1, 1);
            fetchEvents();
        };

        const nextMonth = () => {
            currentMonth.value = new Date(currentMonth.value.getFullYear(), currentMonth.value.getMonth() + 1, 1);
            fetchEvents();
        };

        const fetchEvents = async () => {
            isLoading.value = true;
            try {
                const res = await fetch('/api/calendar/events');
                if (res.ok) events.value = await res.json();
            } catch (e) { console.error(e); }
            finally { isLoading.value = false; }
        };

        const schedulePost = async () => {
            const { value: formValues } = await Swal.fire({
                title: 'Lên lịch xuất bản',
                html: `
                    <input id="swal-postid" class="swal2-input" placeholder="Post ID" type="number" />
                    <input id="swal-date" class="swal2-input" type="datetime-local" />
                `,
                focusConfirm: false,
                showCancelButton: true, confirmButtonText: 'Lên lịch', cancelButtonText: 'Hủy', confirmButtonColor: '#3b82f6',
                preConfirm: () => {
                    const postId = document.getElementById('swal-postid').value;
                    const publishDate = document.getElementById('swal-date').value;
                    if (!postId || !publishDate) { Swal.showValidationMessage('Nhập đầy đủ!'); return false; }
                    if (new Date(publishDate) <= new Date()) { Swal.showValidationMessage('Phải trong tương lai!'); return false; }
                    return { postId: parseInt(postId), publishDate };
                }
            });
            if (!formValues) return;
            try {
                const res = await fetch('/api/calendar/schedule', {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(formValues)
                });
                const data = await res.json();
                if (res.ok) { Swal.fire({ icon: 'success', title: data.message, timer: 2000, showConfirmButton: false }); fetchEvents(); }
                else Swal.fire('Lỗi', data.message, 'error');
            } catch (e) { Swal.fire('Lỗi', 'Kết nối server thất bại', 'error'); }
        };

        onMounted(fetchEvents);

        return { events, currentMonth, isLoading, monthName, calendarDays, getEventsForDay, isToday, prevMonth, nextMonth, fetchEvents, schedulePost, statusLabels };
    },
    template: `
        <div>
            <div class="mb-6 flex items-center justify-between">
                <div>
                    <h2 class="text-2xl font-bold text-black">Lịch nội dung</h2>
                    <p class="text-sm text-slate-500 mt-1">Lên lịch và quản lý bài viết</p>
                </div>
                <button @click="schedulePost"
                    class="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg text-sm hover:bg-opacity-90 transition">
                    <i class="bi bi-plus-lg"></i> Lên lịch mới
                </button>
            </div>

            <!-- Month Navigation -->
            <div class="bg-white rounded-lg border border-stroke p-4 mb-4">
                <div class="flex items-center justify-between mb-4">
                    <button @click="prevMonth" class="p-2 hover:bg-gray-100 rounded-lg transition">
                        <i class="bi bi-chevron-left text-lg"></i>
                    </button>
                    <h3 class="text-lg font-bold text-black capitalize">{{ monthName() }}</h3>
                    <button @click="nextMonth" class="p-2 hover:bg-gray-100 rounded-lg transition">
                        <i class="bi bi-chevron-right text-lg"></i>
                    </button>
                </div>

                <!-- Day Headers -->
                <div class="grid grid-cols-7 gap-1 mb-2">
                    <div v-for="d in ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7']" :key="d"
                        class="text-center text-xs font-medium text-slate-500 py-2">{{ d }}</div>
                </div>

                <!-- Calendar Grid -->
                <div class="grid grid-cols-7 gap-1">
                    <div v-for="(day, i) in calendarDays()" :key="i"
                        class="min-h-[100px] border border-stroke rounded-lg p-1.5 relative"
                        :class="{ 'bg-gray-50': !day, 'ring-2 ring-primary': isToday(day) }">
                        <span v-if="day" class="text-xs font-medium" :class="isToday(day) ? 'text-primary font-bold' : 'text-slate-600'">
                            {{ day }}
                        </span>
                        <!-- Events -->
                        <div v-for="event in getEventsForDay(day)" :key="event.id"
                            class="mt-1 px-1.5 py-0.5 rounded text-[10px] leading-tight truncate cursor-pointer hover:opacity-80 transition"
                            :style="{ backgroundColor: event.color + '20', color: event.color, borderLeft: '2px solid ' + event.color }"
                            :title="event.title + ' (' + (statusLabels[event.status] || event.status) + ')'">
                            {{ event.title }}
                        </div>
                    </div>
                </div>
            </div>

            <!-- Events Legend -->
            <div class="bg-white rounded-lg border border-stroke p-4">
                <h4 class="font-bold text-black mb-3">Chú thích</h4>
                <div class="flex flex-wrap gap-4 text-xs">
                    <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-[#10b981]"></span> Đã phát hành</div>
                    <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-[#3b82f6]"></span> Đã lên lịch</div>
                    <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-[#f59e0b]"></span> Đang duyệt</div>
                    <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-[#6b7280]"></span> Nháp</div>
                    <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-[#ef4444]"></span> Từ chối</div>
                </div>
            </div>
        </div>
    `
};
