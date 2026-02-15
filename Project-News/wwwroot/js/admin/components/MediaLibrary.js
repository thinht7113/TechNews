const { ref, onMounted } = Vue;

export default {
    setup() {
        const images = ref([]);
        const loading = ref(true);
        const uploading = ref(false);

        const fetchImages = async () => {
            loading.value = true;
            try {
                const res = await fetch('/api/media/getall');
                images.value = await res.json();
            } finally { loading.value = false; }
        };

        const handleUpload = async (e) => {
            const files = e.target.files;
            if (!files.length) return;

            uploading.value = true;
            const formData = new FormData();
            for (let i = 0; i < files.length; i++) {
                formData.append('files', files[i]);
            }

            try {
                const res = await fetch('/api/media/upload', { method: 'POST', body: formData });
                if (res.ok) {
                    Swal.fire({ toast: true, position: 'top-end', icon: 'success', title: 'Upload thành công', showConfirmButton: false, timer: 1500 });
                    fetchImages();
                }
            } catch (err) { Swal.fire('Lỗi', 'Upload thất bại', 'error'); }
            finally { uploading.value = false; e.target.value = ''; }
        };

        const copyUrl = (url) => {
            navigator.clipboard.writeText(url);
            Swal.fire({ toast: true, position: 'top-end', icon: 'success', title: 'Đã copy URL', showConfirmButton: false, timer: 1000 });
        };

        const deleteImage = async (url) => {
            const result = await Swal.fire({
                title: 'Xóa ảnh?',
                text: "Không thể hoàn tác!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: 'Xóa'
            });

            if (result.isConfirmed) {
                try {
                    const res = await fetch('/api/media/delete', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ url })
                    });
                    if (res.ok) {
                        images.value = images.value.filter(img => img.url !== url);
                        Swal.fire('Đã xóa', '', 'success');
                    }
                } catch (e) { }
            }
        };

        onMounted(fetchImages);
        return { images, loading, handleUpload, uploading, copyUrl, deleteImage };
    },
    template: `
        <div>
            <div class="flex justify-between items-center mb-6">
                 <h2 class="text-2xl font-bold text-black">Thư viện Media</h2>
                 <label class="bg-primary text-white py-2 px-4 rounded cursor-pointer hover:bg-highlight transition flex items-center gap-2">
                     <i v-if="uploading" class="bi bi-arrow-repeat animate-spin"></i>
                     <i v-else class="bi bi-cloud-upload"></i>
                     <span>Upload Ảnh</span>
                     <input type="file" multiple accept="image/*" class="hidden" @change="handleUpload" :disabled="uploading" />
                 </label>
            </div>

            <div v-if="loading">Đang tải...</div>
            <div v-else class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4">
                <div v-for="img in images" :key="img.name" class="p-2 border rounded bg-white group relative hover:shadow-lg transition">
                    <div class="aspect-square flex items-center justify-center bg-gray-50 overflow-hidden rounded mb-2">
                         <img :src="img.url" class="max-w-full max-h-full object-contain" />
                    </div>
                    <div class="text-xs text-slate-500 truncate px-1">{{ img.name }}</div>
                    
                    <!-- Overlay Actions -->
                    <div class="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition flex flex-col items-center justify-center gap-2 rounded">
                        <button @click="copyUrl(img.url)" class="bg-white text-xs font-bold px-3 py-1 rounded hover:bg-gray-100">Copy URL</button>
                        <button @click="deleteImage(img.url)" class="bg-red-500 text-white text-xs font-bold px-3 py-1 rounded hover:bg-red-600">Xóa</button>
                    </div>
                </div>
            </div>
            
            <div v-if="images.length === 0 && !loading" class="text-center py-12 text-gray-500 border-2 border-dashed rounded-lg">
                Chưa có ảnh nào. Hãy upload ngay!
            </div>
        </div>
    `
};
