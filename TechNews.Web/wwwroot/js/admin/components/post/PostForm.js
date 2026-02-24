import { ClassicEditor } from 'ckeditor5';
import { editorConfig } from '../../utils/editor-config.js';

const { ref, reactive, onMounted, computed, watch } = Vue;
const { useRoute, useRouter } = VueRouter;

export default {
    setup() {
        const route = useRoute();
        const router = useRouter();
        const isEdit = computed(() => !!route.params.id);

        const form = reactive({
            title: '',
            content: '',
            shortDescription: '',
            categoryId: '',
            status: 1,
            metaTitle: '',
            metaDescription: '',
            tags: '',
            thumbnailFile: null,
            thumbnailUrl: '' // For display in edit mode
        });

        const categories = ref([]);
        const revisions = ref([]);
        const previewImage = ref(null);
        let editorInstance = null;
        const isSubmitting = ref(false);
        const isLoading = ref(false);

        const seoScore = computed(() => {
            let score = 0;
            const titleLen = form.title ? form.title.length : 0;
            const descLen = form.shortDescription ? form.shortDescription.length : 0;
            const contentLen = form.content ? form.content.split(/\s+/).length : 0;

            if (titleLen >= 40 && titleLen <= 70) score += 20;
            else if (titleLen > 10) score += 10;

            if (descLen >= 120 && descLen <= 160) score += 20;
            else if (descLen > 50) score += 10;

            if (contentLen > 300) score += 20;
            else if (contentLen > 100) score += 10;

            if (form.title && form.shortDescription) {
                const titleWords = form.title.toLowerCase().split(' ').filter(w => w.length > 3);
                const matchCount = titleWords.filter(w => form.shortDescription.toLowerCase().includes(w)).length;
                if (matchCount >= 2) score += 20;
                else if (matchCount >= 1) score += 10;
            }

            if (previewImage.value || form.thumbnailUrl) score += 20;

            return score;
        });

        const fetchRevisions = async () => {
            if (!isEdit.value) return;
            try {
                const res = await fetch(`/api/post/revisions/${route.params.id}`);
                if (res.ok) revisions.value = await res.json();
            } catch (e) { console.error(e); }
        };

        const restoreRevision = async (revId) => {
            const result = await Swal.fire({
                title: 'Khôi phục phiên bản?',
                text: "Dữ liệu hiện tại sẽ được lưu thành phiên bản mới trước khi khôi phục.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Khôi phục',
                cancelButtonText: 'Hủy'
            });

            if (result.isConfirmed) {
                try {
                    const res = await fetch(`/api/post/restore/${revId}`, { method: 'POST' });
                    if (res.ok) {
                        Swal.fire('Thành công', 'Đã khôi phục phiên bản cũ', 'success').then(() => {
                            location.reload(); // Reload to get restored data
                        });
                    }
                } catch (e) {
                    Swal.fire('Lỗi', 'Không thể khôi phục', 'error');
                }
            }
        };

        const initEditor = () => {
            ClassicEditor
                .create(document.querySelector('#editor'), editorConfig)
                .then(editor => {
                    editorInstance = editor;
                    if (form.content) editor.setData(form.content);
                    editor.model.document.on('change:data', () => { form.content = editor.getData(); });
                })
                .catch(err => console.error(err));
        };

        onMounted(async () => {
            const resCat = await fetch('/api/category/getall');
            categories.value = await resCat.json();

            if (isEdit.value) {
                isLoading.value = true;
                const res = await fetch(`/api/post/get/${route.params.id}`);
                if (res.ok) {
                    const data = await res.json();

                    form.title = data.title;
                    form.content = data.content;
                    form.shortDescription = data.shortDescription;
                    form.categoryId = data.categoryId;
                    form.status = data.status;
                    form.metaTitle = data.metaTitle;
                    form.metaDescription = data.metaDescription;
                    form.tags = data.tags;
                    form.thumbnailUrl = data.thumbnailUrl;

                    if (data.thumbnailUrl) previewImage.value = data.thumbnailUrl;

                    fetchRevisions();
                }
                isLoading.value = false;
            } else {
                if (categories.value.length > 0) form.categoryId = categories.value[0].id;
            }

            requestAnimationFrame(() => initEditor());
        });

        const fileInputRef = ref(null);

        const triggerFileInput = () => {
            fileInputRef.value.click();
        };

        const handleDrop = (e) => {
            const files = e.dataTransfer.files;
            if (files && files.length > 0) {
                const file = files[0];
                form.thumbnailFile = file;
                const reader = new FileReader();
                reader.onload = (ev) => previewImage.value = ev.target.result;
                reader.readAsDataURL(file);
            } else {
                const html = e.dataTransfer.getData('text/html');
                if (html) {
                    const doc = new DOMParser().parseFromString(html, 'text/html');
                    const img = doc.querySelector('img');
                    if (img && img.src) {
                        form.thumbnailUrl = img.getAttribute('src');
                        previewImage.value = img.src;
                        form.thumbnailFile = null;
                    }
                } else {
                    const uri = e.dataTransfer.getData('text/uri-list');
                    if (uri) {
                        form.thumbnailUrl = uri;
                        previewImage.value = uri;
                        form.thumbnailFile = null;
                    }
                }
            }
        };

        const handleFileUpload = (event) => {
            const file = event.target.files[0];
            if (!file) return;
            form.thumbnailFile = file;
            const reader = new FileReader();
            reader.onload = (e) => previewImage.value = e.target.result;
            reader.readAsDataURL(file);
        };

        const removeImage = () => {
            form.thumbnailFile = null;
            previewImage.value = null;
            form.thumbnailUrl = '';
            if (fileInputRef.value) fileInputRef.value.value = '';
        };

        const submitForm = async () => {
            if (!form.title) {
                Swal.fire('Lỗi', 'Vui lòng nhập tiêu đề bài viết', 'error');
                return;
            }
            isSubmitting.value = true;
            const formData = new FormData();
            formData.append('Title', form.title);
            formData.append('ShortDescription', form.shortDescription || '');
            formData.append('Content', form.content || '');
            formData.append('CategoryId', form.categoryId);
            formData.append('Status', form.status);
            formData.append('MetaTitle', form.metaTitle || '');
            formData.append('MetaDescription', form.metaDescription || '');
            formData.append('Tags', form.tags || '');

            if (form.thumbnailFile) formData.append('ThumbnailFile', form.thumbnailFile);
            else if (form.thumbnailUrl) formData.append('ThumbnailUrl', form.thumbnailUrl);

            const url = isEdit.value ? `/api/post/update/${route.params.id}` : '/api/post/create';

            try {
                const res = await fetch(url, { method: 'POST', body: formData });
                if (res.ok) {
                    Swal.fire({ title: isEdit.value ? 'Đã cập nhật!' : 'Đã xuất bản!', text: 'Bài viết đã được lưu thành công.', icon: 'success', confirmButtonColor: '#3C50E0' })
                        .then(() => {
                            if (isEdit.value) fetchRevisions(); // Refresh revisions
                            else router.push('/Admin/Post');
                        });
                } else {
                    Swal.fire('Lỗi!', 'Có lỗi xảy ra khi lưu.', 'error');
                }
            } catch (err) {
                Swal.fire('Lỗi server!', 'Vui lòng thử lại sau.', 'error');
            } finally {
                isSubmitting.value = false;
            }
        };

        return { form, categories, previewImage, handleFileUpload, removeImage, submitForm, isSubmitting, isEdit, isLoading, seoScore, revisions, restoreRevision, handleDrop, triggerFileInput, fileInputRef };
    },
    template: `
        <div>
            <div class="mb-4 flex items-center justify-between">
                <div class="flex items-center gap-4">
                    <router-link to="/Admin/Post" class="text-black hover:text-primary">
                        <i class="bi bi-arrow-left text-2xl"></i>
                    </router-link>
                    <h2 class="text-2xl font-bold text-black">{{ isEdit ? 'Chỉnh sửa bài viết' : 'Thêm bài viết mới' }}</h2>
                </div>
            </div>
            <div v-if="isLoading">Đang tải dữ liệu...</div>
            <form v-else @submit.prevent="submitForm" class="grid grid-cols-1 gap-6 lg:grid-cols-3">
                <div class="lg:col-span-2 flex flex-col gap-6">
                     <div class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <input v-model="form.title" type="text" placeholder="Thêm tiêu đề" class="w-full text-2xl font-bold border-none outline-none placeholder-slate-400" />
                     </div>
                     <div class="rounded-sm border border-stroke bg-white shadow-sm">
                        <div id="editor" class="min-h-[400px]"></div>
                     </div>
                     <div class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <h4 class="font-bold text-black mb-2">Tóm tắt</h4>
                        <textarea v-model="form.shortDescription" rows="3" class="w-full border border-stroke rounded p-2 text-sm focus:border-primary outline-none"></textarea>
                     </div>
                     <div class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <h4 class="font-bold text-black mb-2">SEO Meta (Tùy chọn)</h4>
                         <div class="grid grid-cols-1 gap-4">
                            <input v-model="form.metaTitle" type="text" placeholder="Meta Title" class="w-full rounded border border-stroke px-4 py-2 text-sm focus:border-primary outline-none" />
                            <textarea v-model="form.metaDescription" rows="2" placeholder="Meta Description" class="w-full rounded border border-stroke px-4 py-2 text-sm focus:border-primary outline-none"></textarea>
                            <input v-model="form.tags" type="text" placeholder="Tags (cách nhau bởi dấu phẩy)" class="w-full rounded border border-stroke px-4 py-2 text-sm focus:border-primary outline-none" />
                         </div>
                     </div>
                </div>
                <!-- Sidebar -->
                <div class="flex flex-col gap-6">
                    <div class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <h4 class="font-bold text-black mb-3 border-b border-stroke pb-2">Đăng bài</h4>
                        <div class="flex justify-between items-center pt-3 border-t border-stroke">
                             <span class="text-sm font-bold text-black">{{ form.status === 1 ? 'Công khai' : 'Nháp' }}</span>
                            <button type="submit" :disabled="isSubmitting" class="bg-primary text-white py-2 px-4 rounded font-medium hover:bg-opacity-90 disabled:opacity-70">
                                {{ isSubmitting ? 'Lưu...' : (isEdit ? 'Cập nhật' : 'Đăng ngay') }}
                            </button>
                        </div>
                    </div>

                    <!-- SEO Score Widget -->
                    <div class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <h4 class="font-bold text-black mb-3">SEO Score</h4>
                        <div class="flex items-center gap-4 mb-3">
                            <div class="w-full bg-gray-200 rounded-full h-2.5 dark:bg-gray-700">
                                <div class="bg-green-600 h-2.5 rounded-full transition-all duration-500" :style="{ width: seoScore + '%' }"></div>
                            </div>
                            <span class="text-sm font-bold text-black">{{ seoScore }}/100</span>
                        </div>
                        <ul class="text-xs space-y-1 text-slate-500">
                            <li :class="{'text-green-600': form.title && form.title.length >= 40}">
                                <i class="bi" :class="form.title && form.title.length >= 40 ? 'bi-check-circle-fill' : 'bi-circle'"></i> Tiêu đề 40-70 ký tự
                            </li>
                            <li :class="{'text-green-600': form.shortDescription && form.shortDescription.length >= 120}">
                                <i class="bi" :class="form.shortDescription && form.shortDescription.length >= 120 ? 'bi-check-circle-fill' : 'bi-circle'"></i> Mô tả 120-160 ký tự
                            </li>
                             <li :class="{'text-green-600': form.content && form.content.length > 500}">
                                <i class="bi" :class="form.content && form.content.length > 500 ? 'bi-check-circle-fill' : 'bi-circle'"></i> Nội dung > 300 từ
                            </li>
                             <li :class="{'text-green-600': previewImage}">
                                <i class="bi" :class="previewImage ? 'bi-check-circle-fill' : 'bi-circle'"></i> Có ảnh đại diện
                            </li>
                        </ul>
                    </div>

                    <!-- Revisions Widget -->
                    <div v-if="isEdit && revisions.length > 0" class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <h4 class="font-bold text-black mb-3">Lịch sử chỉnh sửa</h4>
                        <ul class="space-y-2 max-h-60 overflow-y-auto">
                            <li v-for="rev in revisions" :key="rev.id" class="flex justify-between items-center text-xs border-b border-stroke pb-2 last:border-0">
                                <div>
                                    <p class="font-medium text-black">v{{ rev.version }}</p>
                                    <span class="text-slate-500">{{ new Date(rev.modifiedDate).toLocaleString('vi-VN') }}</span>
                                </div>
                                <button @click="restoreRevision(rev.id)" class="text-primary hover:underline">Khôi phục</button>
                            </li>
                        </ul>
                    </div>

                     <div class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <h4 class="font-bold text-black mb-3">Chuyên mục</h4>
                        <div class="max-h-40 overflow-y-auto border border-stroke rounded p-2 bg-gray-50">
                            <div v-for="cat in categories" :key="cat.id" class="flex items-center mb-2 last:mb-0">
                                <input type="radio" :id="'cat-'+cat.id" :value="cat.id" v-model="form.categoryId" class="mr-2" />
                                <label :for="'cat-'+cat.id" class="text-sm cursor-pointer select-none">{{ cat.name }}</label>
                            </div>
                        </div>
                    </div>
                     <div class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <h4 class="font-bold text-black mb-3">Ảnh đại diện</h4>
                        <div class="border-2 border-dashed border-gray-300 rounded-lg p-4 text-center hover:bg-gray-50 transition relative cursor-pointer"
                             @dragover.prevent @drop.prevent="handleDrop" @click="triggerFileInput">
                            <input type="file" ref="fileInputRef" @change="handleFileUpload" class="hidden" accept="image/*" />
                            <div v-if="previewImage" class="relative z-10">
                                <img :src="previewImage" class="w-full h-auto rounded mb-2" />
                                <button type="button" @click.stop="removeImage" class="text-xs text-red-500 hover:underline">Xóa ảnh</button>
                            </div>
                            <div v-else class="text-gray-500 text-sm">
                                <i class="bi bi-image text-2xl mb-2 block"></i>
                                Kéo thả ảnh từ bài viết hoặc click để chọn
                            </div>
                        </div>
                    </div>
                </div>
            </form>
        </div>
    `
};