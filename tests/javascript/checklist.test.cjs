const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

function loadScripts() {
    const context = vm.createContext({
        FormData, File, Blob,
        document: {
            createElement() {
                return {
                    dataset: new Proxy({}, {
                        set(target, key, value) {
                            target[key] = String(value);
                            return true;
                        }
                    })
                };
            }
        }
    });
    for (const file of ['table.js', 'api.js']) {
        vm.runInContext(fs.readFileSync(path.join(__dirname, '../../wwwroot/js', file), 'utf8'), context);
    }
    return vm.runInContext('({ Table, objectToFormData })', context);
}

for (const [name, source, expected] of [
    ['manual addition', null, null],
    ['From text', { title: 'Checklist item' }, null],
    ['explicit null ID', { id: null }, null],
    ['existing item', { id: 42 }, '42']
]) {
    test(`${name}: ID survives row creation, reading, and FormData serialization`, () => {
        const { Table, objectToFormData } = loadScripts();
        const table = Object.create(Table.prototype);
        table._columns = [{ id: 'id', type: 'hidden' }];
        const row = table._createRow(source);
        const item = table._readRow(row);
        const form = objectToFormData({ check_items: [item] });
        assert.equal(form.get('check_items[0].id'), expected);
    });
}

test('invalid checklist IDs are omitted instead of sending NaN', () => {
    const { Table, objectToFormData } = loadScripts();
    const table = Object.create(Table.prototype);
    table._columns = [{ id: 'id', type: 'hidden' }];
    for (const id of ['', 'undefined', 'null', 'NaN', 'abc', '42abc', '1.5', '-1', undefined]) {
        const item = table._readRow({ dataset: { id } });
        assert.equal(item.id, null);
        assert.equal(objectToFormData({ check_items: [item] }).has('check_items[0].id'), false);
    }
});

test('FormData preserves cleared description, checklist text, booleans, and array indexes', () => {
    const { objectToFormData } = loadScripts();
    const form = objectToFormData({
        description: '',
        check_items: [
            { id: null, title: 'First', is_completed: false },
            { id: 42, title: 'Second', is_completed: true }
        ]
    });
    assert.deepEqual([...form.entries()], [
        ['description', ''],
        ['check_items[0].title', 'First'],
        ['check_items[0].is_completed', 'false'],
        ['check_items[1].id', '42'],
        ['check_items[1].title', 'Second'],
        ['check_items[1].is_completed', 'true']
    ]);
});

test('FormData omits missing values and preserves uploaded files', () => {
    const { objectToFormData } = loadScripts();
    const file = new File(['content'], 'note.txt', { type: 'text/plain' });
    const form = objectToFormData({ description: null, category_id: undefined, files: [{ content: file }] });
    assert.equal(form.has('description'), false);
    assert.equal(form.has('category_id'), false);
    assert.equal(form.get('files[0].content'), file);
});
