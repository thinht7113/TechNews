import { ClassicEditor } from 'ckeditor5';
import { editorConfig } from '../../utils/editor-config.js';

const { ref, reactive, onMounted, computed, watch } = Vue;
const { useRoute, useRouter } = VueRouter;

// Convert Markdown from AI responses to HTML for CKEditor
function markdownToHtml(md) {
    if (!md) return '';
    let html = md;
    // Code blocks (``` ... ```)
    html = html.replace(/```([\s\S]*?)```/g, '<pre><code>$1</code></pre>');
    // Headings (### > ## > #)
    html = html.replace(/^### (.+)$/gm, '<h3>$1</h3>');
    html = html.replace(/^## (.+)$/gm, '<h2>$1</h2>');
    html = html.replace(/^# (.+)$/gm, '<h1>$1</h1>');
    // Bold & Italic
    html = html.replace(/\*\*\*(.+?)\*\*\*/g, '<strong><em>$1</em></strong>');
    html = html.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
    html = html.replace(/\*(.+?)\*/g, '<em>$1</em>');
    // Unordered lists
    html = html.replace(/^[\-\*] (.+)$/gm, '<li>$1</li>');
    html = html.replace(/(<li>.*<\/li>\n?)+/g, '<ul>$&</ul>');
    // Ordered lists
    html = html.replace(/^\d+\. (.+)$/gm, '<li>$1</li>');
    // Horizontal rules
    html = html.replace(/^---+$/gm, '<hr>');
    // Inline code
    html = html.replace(/`([^`]+)`/g, '<code>$1</code>');
    // Paragraphs: wrap remaining plain text lines
    html = html.split('\n').map(line => {
        line = line.trim();
        if (!line) return '';
        if (/^<(h[1-6]|ul|ol|li|pre|hr|blockquote)/.test(line)) return line;
        return '<p>' + line + '</p>';
    }).join('\n');
    // Clean up empty paragraphs
    html = html.replace(/<p><\/p>/g, '');
    return html;
}

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

        // === AI STATE ===
        const aiConfigured = ref(false);
        const aiLoading = ref(false);
        const aiResult = ref('');
        const aiSuggestedTags = ref([]);
        const aiSuggestedTitles = ref([]);
        const showAiPanel = ref(false);

        // === SEO ANALYSIS STATE ===
        const seoAnalysis = ref(null);
        const seoLoading = ref(false);
        const focusKeyword = ref('');

        // === Check AI Status ===
        const checkAiStatus = async () => {
            try {
                const res = await fetch('/api/ai/status');
                if (res.ok) {
                    const data = await res.json();
                    aiConfigured.value = data.configured;
                }
            } catch (e) { console.error('AI status check failed', e); }
        };

        // === AI Functions ===
        const aiGenerate = async () => {
            const prompt = await Swal.fire({
                title: 'Nhập chủ đề bài viết',
                input: 'textarea',
                inputPlaceholder: 'Ví dụ: Xu hướng AI trong năm 2026...',
                showCancelButton: true,
                confirmButtonText: 'Sinh nội dung',
                cancelButtonText: 'Hủy',
                confirmButtonColor: '#9f224e'
            });
            if (!prompt.isConfirmed || !prompt.value) return;
            aiLoading.value = true;
            try {
                const res = await fetch('/api/ai/generate', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ prompt: prompt.value, context: form.title })
                });
                const data = await res.json();
                if (data.success && editorInstance) {
                    const htmlContent = markdownToHtml(data.content);
                    editorInstance.setData(htmlContent);
                    form.content = htmlContent;
                    Swal.fire({ icon: 'success', title: 'Đã sinh nội dung!', timer: 1500, showConfirmButton: false });
                } else {
                    Swal.fire('Lỗi', data.message || 'Không thể sinh nội dung', 'error');
                }
            } catch (e) { Swal.fire('Lỗi', 'Kết nối AI thất bại', 'error'); }
            finally { aiLoading.value = false; }
        };

        const aiSummarize = async () => {
            if (!form.content) { Swal.fire('Cảnh báo', 'Chưa có nội dung để tóm tắt', 'warning'); return; }
            aiLoading.value = true;
            try {
                const res = await fetch('/api/ai/summarize', {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ content: form.content, maxLength: 200 })
                });
                const data = await res.json();
                if (data.success) {
                    form.shortDescription = data.summary;
                    Swal.fire({ icon: 'success', title: 'Đã tạo tóm tắt!', timer: 1500, showConfirmButton: false });
                }
            } catch (e) { Swal.fire('Lỗi', 'Không thể tóm tắt', 'error'); }
            finally { aiLoading.value = false; }
        };

        const aiSuggestTags = async () => {
            if (!form.content) { Swal.fire('Cảnh báo', 'Chưa có nội dung để phân tích', 'warning'); return; }
            aiLoading.value = true;
            try {
                const res = await fetch('/api/ai/suggest-tags', {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ content: form.content })
                });
                const data = await res.json();
                if (data.success) { aiSuggestedTags.value = data.tags; }
            } catch (e) { Swal.fire('Lỗi', 'Không thể gợi ý tags', 'error'); }
            finally { aiLoading.value = false; }
        };

        const applyTag = (tag) => {
            const current = form.tags ? form.tags.split(',').map(t => t.trim()) : [];
            if (!current.includes(tag)) {
                current.push(tag);
                form.tags = current.join(', ');
            }
        };

        const aiSuggestTitles = async () => {
            if (!form.content) { Swal.fire('Cảnh báo', 'Chưa có nội dung', 'warning'); return; }
            aiLoading.value = true;
            try {
                const res = await fetch('/api/ai/suggest-titles', {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ content: form.content })
                });
                const data = await res.json();
                if (data.success) { aiSuggestedTitles.value = data.titles; }
            } catch (e) { Swal.fire('Lỗi', 'Không thể gợi ý tiêu đề', 'error'); }
            finally { aiLoading.value = false; }
        };

        const applyTitle = (title) => { form.title = title; };

        const aiImproveWriting = async () => {
            if (!form.content) { Swal.fire('Cảnh báo', 'Chưa có nội dung', 'warning'); return; }
            aiLoading.value = true;
            try {
                const res = await fetch('/api/ai/improve', {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ content: form.content })
                });
                const data = await res.json();
                if (data.success && editorInstance) {
                    const htmlContent = markdownToHtml(data.content);
                    editorInstance.setData(htmlContent);
                    form.content = htmlContent;
                    Swal.fire({ icon: 'success', title: 'Đã cải thiện văn phong!', timer: 1500, showConfirmButton: false });
                }
            } catch (e) { Swal.fire('Lỗi', 'Không thể cải thiện', 'error'); }
            finally { aiLoading.value = false; }
        };

        // === SEO Analysis ===
        const analyzeSeo = async () => {
            if (!form.title) return;
            seoLoading.value = true;
            try {
                const res = await fetch('/api/seo/analyze', {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        title: form.title,
                        content: form.content || '',
                        metaTitle: form.metaTitle,
                        metaDescription: form.metaDescription || form.shortDescription,
                        focusKeyword: focusKeyword.value
                    })
                });
                if (res.ok) { seoAnalysis.value = await res.json(); }
            } catch (e) { console.error('SEO analysis failed', e); }
            finally { seoLoading.value = false; }
        };

        const seoScore = computed(() => seoAnalysis.value ? seoAnalysis.value.overallScore : 0);

        const seoScoreColor = computed(() => {
            const score = seoScore.value;
            if (score >= 80) return '#10b981';
            if (score >= 50) return '#f59e0b';
            return '#ef4444';
        });

        // legacy fallback if SEO not analyzed yet  
        const basicSeoScore = computed(() => {
            let score = 0;
            const titleLen = form.title ? form.title.length : 0;
            const descLen = form.shortDescription ? form.shortDescription.length : 0;
            const contentLen = form.content ? form.content.split(/\s+/).length : 0;
            if (titleLen >= 40 && titleLen <= 70) score += 20; else if (titleLen > 10) score += 10;
            if (descLen >= 120 && descLen <= 160) score += 20; else if (descLen > 50) score += 10;
            if (contentLen > 300) score += 20; else if (contentLen > 100) score += 10;
            if (form.title && form.shortDescription) {
                const titleWords = form.title.toLowerCase().split(' ').filter(w => w.length > 3);
                const matchCount = titleWords.filter(w => form.shortDescription.toLowerCase().includes(w)).length;
                if (matchCount >= 2) score += 20; else if (matchCount >= 1) score += 10;
            }
            if (previewImage.value || form.thumbnailUrl) score += 20;
            return score;
        });

        const displayScore = computed(() => seoAnalysis.value ? seoScore.value : basicSeoScore.value);

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
                icon: 'question', showCancelButton: true, confirmButtonText: 'Khôi phục', cancelButtonText: 'Hủy'
            });
            if (result.isConfirmed) {
                try {
                    const res = await fetch(`/api/post/restore/${revId}`, { method: 'POST' });
                    if (res.ok) Swal.fire('Thành công', 'Đã khôi phục phiên bản cũ', 'success').then(() => location.reload());
                } catch (e) { Swal.fire('Lỗi', 'Không thể khôi phục', 'error'); }
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

            checkAiStatus();

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
        const triggerFileInput = () => { fileInputRef.value.click(); };

        const handleDrop = (e) => {
            const files = e.dataTransfer.files;
            if (files && files.length > 0) {
                const file = files[0]; form.thumbnailFile = file;
                const reader = new FileReader();
                reader.onload = (ev) => previewImage.value = ev.target.result;
                reader.readAsDataURL(file);
            } else {
                const html = e.dataTransfer.getData('text/html');
                if (html) {
                    const doc = new DOMParser().parseFromString(html, 'text/html');
                    const img = doc.querySelector('img');
                    if (img && img.src) { form.thumbnailUrl = img.getAttribute('src'); previewImage.value = img.src; form.thumbnailFile = null; }
                } else {
                    const uri = e.dataTransfer.getData('text/uri-list');
                    if (uri) { form.thumbnailUrl = uri; previewImage.value = uri; form.thumbnailFile = null; }
                }
            }
        };

        const handleFileUpload = (event) => {
            const file = event.target.files[0]; if (!file) return;
            form.thumbnailFile = file;
            const reader = new FileReader();
            reader.onload = (e) => previewImage.value = e.target.result;
            reader.readAsDataURL(file);
        };

        const removeImage = () => { form.thumbnailFile = null; previewImage.value = null; form.thumbnailUrl = ''; if (fileInputRef.value) fileInputRef.value.value = ''; };

        const submitForm = async () => {
            if (!form.title) { Swal.fire('Lỗi', 'Vui lòng nhập tiêu đề bài viết', 'error'); return; }
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
                        .then(() => { if (isEdit.value) fetchRevisions(); else router.push('/Admin/Post'); });
                } else { Swal.fire('Lỗi!', 'Có lỗi xảy ra khi lưu.', 'error'); }
            } catch (err) { Swal.fire('Lỗi server!', 'Vui lòng thử lại sau.', 'error'); }
            finally { isSubmitting.value = false; }
        };

        return {
            form, categories, previewImage, handleFileUpload, removeImage, submitForm,
            isSubmitting, isEdit, isLoading, revisions, restoreRevision, handleDrop,
            triggerFileInput, fileInputRef,
            // AI
            aiConfigured, aiLoading, aiResult, aiSuggestedTags, aiSuggestedTitles,
            showAiPanel, aiGenerate, aiSummarize, aiSuggestTags, applyTag,
            aiSuggestTitles, applyTitle, aiImproveWriting,
            // SEO
            seoAnalysis, seoLoading, focusKeyword, analyzeSeo, displayScore,
            seoScoreColor, seoScore
        };
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
                <!-- AI Toggle Button -->
                <button v-if="aiConfigured" @click="showAiPanel = !showAiPanel"
                    class="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all duration-300"
                    :class="showAiPanel ? 'bg-gradient-to-r from-purple-600 to-indigo-600 text-white shadow-lg shadow-purple-200' : 'bg-white border border-stroke text-slate-600 hover:border-purple-400 hover:text-purple-600'">
                    <i class="bi bi-stars"></i>
                    {{ showAiPanel ? 'Ẩn AI Assistant' : 'AI Assistant' }}
                </button>
            </div>
            <div v-if="isLoading">Đang tải dữ liệu...</div>
            <form v-else @submit.prevent="submitForm" class="grid grid-cols-1 gap-6" :class="showAiPanel ? 'lg:grid-cols-12' : 'lg:grid-cols-3'">
                <!-- Main Content (Left) -->
                <div :class="showAiPanel ? 'lg:col-span-5' : 'lg:col-span-2'" class="flex flex-col gap-6">
                     <div class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <input v-model="form.title" type="text" placeholder="Thêm tiêu đề" class="w-full text-2xl font-bold border-none outline-none placeholder-slate-400" />
                     </div>
                     <div class="rounded-sm border border-stroke bg-white shadow-sm">
                        <div id="editor" class="min-h-[400px]"></div>
                     </div>
                     <div class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <div class="flex items-center justify-between mb-2">
                            <h4 class="font-bold text-black">Tóm tắt</h4>
                            <button v-if="aiConfigured" type="button" @click="aiSummarize" :disabled="aiLoading"
                                class="text-xs flex items-center gap-1 text-purple-600 hover:text-purple-800 disabled:opacity-50">
                                <i class="bi bi-stars"></i> AI tóm tắt
                            </button>
                        </div>
                        <textarea v-model="form.shortDescription" rows="3" class="w-full border border-stroke rounded p-2 text-sm focus:border-primary outline-none"></textarea>
                     </div>
                     <div class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <h4 class="font-bold text-black mb-2">SEO Meta (Tùy chọn)</h4>
                         <div class="grid grid-cols-1 gap-4">
                            <input v-model="form.metaTitle" type="text" placeholder="Meta Title" class="w-full rounded border border-stroke px-4 py-2 text-sm focus:border-primary outline-none" />
                            <textarea v-model="form.metaDescription" rows="2" placeholder="Meta Description" class="w-full rounded border border-stroke px-4 py-2 text-sm focus:border-primary outline-none"></textarea>
                            <div>
                                <div class="flex items-center justify-between mb-1">
                                    <span class="text-sm text-slate-500">Tags</span>
                                    <button v-if="aiConfigured" type="button" @click="aiSuggestTags" :disabled="aiLoading"
                                        class="text-xs flex items-center gap-1 text-purple-600 hover:text-purple-800 disabled:opacity-50">
                                        <i class="bi bi-stars"></i> AI gợi ý
                                    </button>
                                </div>
                                <input v-model="form.tags" type="text" placeholder="Tags (cách nhau bởi dấu phẩy)" class="w-full rounded border border-stroke px-4 py-2 text-sm focus:border-primary outline-none" />
                                <!-- AI Suggested Tags -->
                                <div v-if="aiSuggestedTags.length > 0" class="flex flex-wrap gap-1 mt-2">
                                    <button v-for="tag in aiSuggestedTags" :key="tag" type="button" @click="applyTag(tag)"
                                        class="px-2 py-0.5 text-xs bg-purple-50 text-purple-700 border border-purple-200 rounded-full hover:bg-purple-100 transition">
                                        + {{ tag }}
                                    </button>
                                </div>
                            </div>
                         </div>
                     </div>
                </div>

                <!-- Sidebar (Right) -->
                <div :class="showAiPanel ? 'lg:col-span-3' : ''" class="flex flex-col gap-6">
                    <div class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <h4 class="font-bold text-black mb-3 border-b border-stroke pb-2">Đăng bài</h4>
                        <div class="flex justify-between items-center pt-3 border-t border-stroke">
                             <span class="text-sm font-bold text-black">{{ form.status === 1 ? 'Công khai' : 'Nháp' }}</span>
                            <button type="submit" :disabled="isSubmitting" class="bg-primary text-white py-2 px-4 rounded font-medium hover:bg-opacity-90 disabled:opacity-70">
                                {{ isSubmitting ? 'Lưu...' : (isEdit ? 'Cập nhật' : 'Đăng ngay') }}
                            </button>
                        </div>
                    </div>

                    <!-- SEO Score Widget (Enhanced) -->
                    <div class="rounded-sm border border-stroke bg-white shadow-sm p-4">
                        <div class="flex items-center justify-between mb-3">
                            <h4 class="font-bold text-black">SEO Score</h4>
                            <button type="button" @click="analyzeSeo" :disabled="seoLoading"
                                class="text-xs flex items-center gap-1 px-2 py-1 bg-blue-50 text-blue-600 rounded hover:bg-blue-100 disabled:opacity-50">
                                <i class="bi" :class="seoLoading ? 'bi-hourglass-split animate-spin' : 'bi-search'"></i>
                                {{ seoLoading ? 'Đang...' : 'Phân tích SEO' }}
                            </button>
                        </div>
                        <!-- Focus Keyword Input -->
                        <input v-model="focusKeyword" type="text" placeholder="Từ khóa chính (focus keyword)"
                            class="w-full rounded border border-stroke px-3 py-1.5 text-xs mb-3 focus:border-blue-400 outline-none" />
                        <!-- Score Bar -->
                        <div class="flex items-center gap-4 mb-3">
                            <div class="w-full bg-gray-200 rounded-full h-2.5">
                                <div class="h-2.5 rounded-full transition-all duration-500" :style="{ width: displayScore + '%', backgroundColor: seoScoreColor }"></div>
                            </div>
                            <span class="text-sm font-bold" :style="{ color: seoScoreColor }">{{ displayScore }}/100</span>
                        </div>
                        <!-- Detailed SEO Checks -->
                        <div v-if="seoAnalysis" class="space-y-2 text-xs">
                            <div v-for="(check, key) in {
                                'Tiêu đề': seoAnalysis.titleAnalysis,
                                'Meta Description': seoAnalysis.metaDescriptionAnalysis,
                                'Từ khóa': seoAnalysis.keywordAnalysis,
                                'Độ dễ đọc': seoAnalysis.readabilityAnalysis,
                                'Heading': seoAnalysis.headingAnalysis,
                                'Hình ảnh': seoAnalysis.imageAnalysis
                            }" :key="key" class="flex items-start gap-2 p-2 rounded" :class="{
                                'bg-green-50': check && check.status === 'good',
                                'bg-amber-50': check && check.status === 'warning',
                                'bg-red-50': check && check.status === 'bad'
                            }">
                                <i class="bi mt-0.5" :class="{
                                    'bi-check-circle-fill text-green-600': check && check.status === 'good',
                                    'bi-exclamation-triangle-fill text-amber-500': check && check.status === 'warning',
                                    'bi-x-circle-fill text-red-500': check && check.status === 'bad'
                                }"></i>
                                <div>
                                    <span class="font-medium text-black">{{ key }}</span>
                                    <p class="text-slate-500 mt-0.5">{{ check ? check.message : '' }}</p>
                                </div>
                            </div>
                            <!-- Suggestions -->
                            <div v-if="seoAnalysis.suggestions && seoAnalysis.suggestions.length" class="mt-3 p-2 bg-blue-50 rounded">
                                <p class="font-medium text-blue-700 mb-1"><i class="bi bi-lightbulb"></i> Gợi ý:</p>
                                <ul class="list-disc list-inside text-blue-600 space-y-0.5">
                                    <li v-for="s in seoAnalysis.suggestions" :key="s">{{ s }}</li>
                                </ul>
                            </div>
                        </div>
                        <!-- Basic fallback checks -->
                        <ul v-else class="text-xs space-y-1 text-slate-500">
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

                <!-- AI Panel (Far Right) - Only visible when toggled -->
                <div v-if="showAiPanel" class="lg:col-span-4 flex flex-col gap-6">
                    <div class="rounded-lg border-2 border-purple-200 bg-gradient-to-b from-purple-50 to-white shadow-sm p-4">
                        <div class="flex items-center gap-2 mb-4">
                            <div class="w-8 h-8 bg-gradient-to-r from-purple-600 to-indigo-600 rounded-lg flex items-center justify-center">
                                <i class="bi bi-stars text-white"></i>
                            </div>
                            <div>
                                <h4 class="font-bold text-black">AI Writing Assistant</h4>
                                <p class="text-xs text-slate-500">Trợ lý viết bài thông minh</p>
                            </div>
                        </div>

                        <div class="space-y-2">
                            <!-- Generate Content -->
                            <button type="button" @click="aiGenerate" :disabled="aiLoading"
                                class="w-full flex items-center gap-3 px-4 py-3 rounded-lg border border-purple-200 bg-white hover:bg-purple-50 hover:border-purple-300 transition text-left group disabled:opacity-50">
                                <i class="bi bi-pencil-square text-purple-600 group-hover:scale-110 transition"></i>
                                <div>
                                    <p class="text-sm font-medium text-black">Sinh nội dung</p>
                                    <p class="text-xs text-slate-400">Nhập chủ đề, AI viết bài cho bạn</p>
                                </div>
                            </button>

                            <!-- Suggest Titles -->
                            <button type="button" @click="aiSuggestTitles" :disabled="aiLoading"
                                class="w-full flex items-center gap-3 px-4 py-3 rounded-lg border border-purple-200 bg-white hover:bg-purple-50 hover:border-purple-300 transition text-left group disabled:opacity-50">
                                <i class="bi bi-lightbulb text-amber-500 group-hover:scale-110 transition"></i>
                                <div>
                                    <p class="text-sm font-medium text-black">Gợi ý tiêu đề</p>
                                    <p class="text-xs text-slate-400">5 tiêu đề SEO-friendly</p>
                                </div>
                            </button>

                            <!-- Improve Writing -->
                            <button type="button" @click="aiImproveWriting" :disabled="aiLoading"
                                class="w-full flex items-center gap-3 px-4 py-3 rounded-lg border border-purple-200 bg-white hover:bg-purple-50 hover:border-purple-300 transition text-left group disabled:opacity-50">
                                <i class="bi bi-magic text-indigo-600 group-hover:scale-110 transition"></i>
                                <div>
                                    <p class="text-sm font-medium text-black">Cải thiện văn phong</p>
                                    <p class="text-xs text-slate-400">Làm nội dung mạch lạc, chuyên nghiệp hơn</p>
                                </div>
                            </button>
                        </div>

                        <!-- Loading Indicator -->
                        <div v-if="aiLoading" class="mt-4 flex items-center justify-center gap-2 text-purple-600">
                            <div class="w-5 h-5 border-2 border-purple-600 border-t-transparent rounded-full animate-spin"></div>
                            <span class="text-sm">AI đang xử lý...</span>
                        </div>

                        <!-- Suggested Titles -->
                        <div v-if="aiSuggestedTitles.length > 0" class="mt-4">
                            <h5 class="text-sm font-medium text-black mb-2"><i class="bi bi-lightbulb text-amber-500"></i> Tiêu đề gợi ý:</h5>
                            <div class="space-y-1">
                                <button v-for="(title, i) in aiSuggestedTitles" :key="i" type="button" @click="applyTitle(title)"
                                    class="w-full text-left text-xs px-3 py-2 rounded border border-slate-200 hover:bg-purple-50 hover:border-purple-300 transition">
                                    {{ i + 1 }}. {{ title }}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </form>
        </div>
    `
};