import Dashboard from './components/Dashboard.js';
import PostList from './components/post/PostList.js';
import PostForm from './components/post/PostForm.js';
import PostTrash from './components/post/PostTrash.js';
import CategoryList from './components/CategoryList.js';
import CategoryForm from './components/CategoryForm.js';
import MediaLibrary from './components/MediaLibrary.js';
import UserList from './components/UserList.js';
import UserForm from './components/UserForm.js';
import Settings from './components/Settings.js';
import System from './components/System.js';
import CommentList from './components/CommentList.js';

import MenuBuilder from './components/MenuBuilder.js';
import TagList from './components/TagList.js';
import NewsletterManager from './components/NewsletterManager.js';

const { createApp } = Vue;
const { createRouter, createWebHistory } = VueRouter;

const routes = [
    { path: '/Admin/Dashboard', component: Dashboard },
    { path: '/Admin/Post', component: PostList },
    { path: '/Admin/Post/Create', component: PostForm },
    { path: '/Admin/Post/Edit/:id', component: PostForm },
    { path: '/Admin/Post/Trash', component: PostTrash },
    { path: '/Admin/Category', component: CategoryList },
    { path: '/Admin/Category/Create', component: CategoryForm },
    { path: '/Admin/Category/Edit/:id', component: CategoryForm },
    { path: '/Admin/Media', component: MediaLibrary },
    { path: '/Admin/User', component: UserList },
    { path: '/Admin/User/Create', component: UserForm },
    { path: '/Admin/User/Edit/:id', component: UserForm },
    { path: '/Admin/Settings', component: Settings },
    { path: '/Admin/System', component: System },
    { path: '/Admin/Comment', component: CommentList },

    { path: '/Admin/Menu', component: MenuBuilder },
    { path: '/Admin/Tag', component: TagList },
    { path: '/Admin/Newsletter', component: NewsletterManager },
    { path: '/:pathMatch(.*)*', redirect: '/Admin/Dashboard' }
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

const app = createApp({});
app.use(router);
app.mount('#app');

console.log('Vue App initialized with Modular Architecture!');