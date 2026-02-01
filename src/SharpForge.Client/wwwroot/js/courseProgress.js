// Course progress tracking using localStorage

const STORAGE_KEY = 'sharpforge-course-progress';

function getProgressData() {
    const data = localStorage.getItem(STORAGE_KEY);
    return data ? JSON.parse(data) : {};
}

function saveProgressData(data) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
}

export function getCompletedLessons(courseId) {
    const data = getProgressData();
    return data[courseId] || [];
}

export function markLessonComplete(courseId, lessonNumber) {
    const data = getProgressData();
    if (!data[courseId]) {
        data[courseId] = [];
    }
    if (!data[courseId].includes(lessonNumber)) {
        data[courseId].push(lessonNumber);
        data[courseId].sort((a, b) => a - b);
    }
    saveProgressData(data);
    return data[courseId];
}

export function markLessonIncomplete(courseId, lessonNumber) {
    const data = getProgressData();
    if (data[courseId]) {
        data[courseId] = data[courseId].filter(l => l !== lessonNumber);
    }
    saveProgressData(data);
    return data[courseId] || [];
}

export function getCompletedLessonCount(courseId) {
    const completed = getCompletedLessons(courseId);
    return completed.length;
}

export function isLessonComplete(courseId, lessonNumber) {
    const completed = getCompletedLessons(courseId);
    return completed.includes(lessonNumber);
}

export function resetCourseProgress(courseId) {
    const data = getProgressData();
    delete data[courseId];
    saveProgressData(data);
}

export function resetAllProgress() {
    localStorage.removeItem(STORAGE_KEY);
}

