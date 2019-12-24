using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IssueTracker.Data;
using IssueTracker.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Services {
    public class IssueCategoryService {
        private readonly ILogger<IssueCategoryService> _logger;
        private readonly IssueTrackerContext _context;

        public IssueCategoryService(ILogger<IssueCategoryService> logger, IssueTrackerContext context) {
            _logger = logger;
            _context = context;
        }

        public IssueCategory Get(object id, bool deleteCheck = false) {
            var item = _context.IssueCategory.Find(id);
            if (deleteCheck && item != null && item.IsDelete == true) return null;
            return item;
        }

        public void Save(IssueCategory data) {
            try {
                IssueCategory persist = _context.IssueCategory.Find(data.Id);
                persist.Name = data.Name;
                persist.Code = data.Code;
                persist.AcceptAllowIssueEmployee = data.AcceptAllowIssueEmployee;
                persist.IsActive = data.IsActive;
                _context.Update(persist);
                _context.SaveChanges();
            } catch (Exception ex) {
                _logger.LogError("", ex);
                throw ex;
            }
        }

        public IssueCategory CreateNode(object parentId, string title, object position = null) {
            IssueCategory node = null;
            IssueCategory parent = _context.IssueCategory.Find(parentId);

            var childs = parent.Childs;
            if (position == null) {
                if (childs.Count() == 0) {
                    position = 1;
                } else {
                    position = childs.Last().Position + 1;
                }
            }

            node = new IssueCategory {
                Name = title,
                Code = "",
                Parent = parent,
                Position = (int)position,
                IsLeafNode = true,
                IsActive = true,
                IsDelete = false
            };
            if (parent != null) {
                parent.IsLeafNode = false;
                _context.Update(parent);
            }
            _context.Add(node);
            _context.SaveChanges();
            return node;
        }

        public IssueCategory RenameNode(object id, string title) {
            IssueCategory node = null;
            node = _context.IssueCategory.Find(id);
            node.Name = title;
            _context.Update(node);
            _context.SaveChanges();
            return node;
        }

        public void RemoveNode(object id) {
            IssueCategory node = _context.IssueCategory.Find(id);

            node.IsDelete = true;
            _context.Update(node);
            if (node.Parent != null) {
                var sibling = node.Parent.Childs.Where(x => x.IsDelete == false);
                if (sibling.Count() == 0) {
                    node.Parent.IsLeafNode = true;
                    _context.Update(node.Parent);
                }
            }
            _context.SaveChanges();
        }

        public IList<IssueCategory> GetChildren(object id) {
            IList<IssueCategory> list = null;
            if (id != null) {
                IssueCategory parent = _context.IssueCategory.Find(id);
                var query = _context.IssueCategory.Where(x => x.Parent == parent && x.IsDelete == false);
                list = query.OrderBy(x => x.Position).ToList();
            } else {
                var query = _context.IssueCategory.Where(x => x.Parent == null && x.IsDelete == false);
                list = query.OrderBy(x => x.Position).ToList();
            }
            return list;
        }

        public IList<IssueCategory> GetSiblings(object id) {
            IssueCategory target = _context.IssueCategory.Find(id);
            if (target == null) {
                return null;
            }

            var query = _context.IssueCategory
                .Where(x => x.Parent == target.Parent && x.IsDelete == false)
                .OrderBy(x => x.Position);
            return query.ToList();
        }

        /**
         * 상품 상품카테고리 설정
         * @param list 상품ID 배열
         */
        public void SetIssueCategory(int[] list, int categoryId, int depth) {
            IssueCategory node = _context.IssueCategory.Find(categoryId);
            if (node == null) throw new Exception("카테고리가 없습니다.");

            var query = from product in _context.Issue
                        where list.Contains(product.Id)
                        select product;
            foreach (var product in query) {
                product.Category = node;
                _context.Update(product);
            }
            _context.SaveChanges();
        }

        /**
         * 상품 상품카테고리 위치이동
         * @param id 카테고리ID
         * @param postion 이동위치
         */
        public void MoveNode(object id, object position) {
            IssueCategory data = _context.IssueCategory.Find(id);
            if (data == null) throw new Exception("대상카테고리가 없습니다.");

            //repoIssueCategory.MoveNode(data, (int)position);
        }

        public void ArrangeAllPosition() {
            _ArrangePosition(null, 1, 0);
        }

        public IList<IssueCategory> Select(int[] list) {
            return _context.IssueCategory.Where(x => list.Contains(x.Id)).ToList();
        }

        private IEnumerable<IssueCategory> _GetChilds(IssueCategory entity, bool withDeleted = false) {
            var query = from p in _context.IssueCategory
                        where p.Parent == entity
                        select p;
            if (withDeleted == false) {
                return query.Where(x => x.IsDelete == false);
            }
            return query;
        }

        // 노드 데이터 삭제
        private void _RemoveNode(IssueCategory entity) {
            entity.IsDelete = true;
            if (entity.Childs != null) {
                foreach (var child in entity.Childs) {
                    _RemoveNode(child);
                }
            }
        }

        // 노드 데이터 저장
        private void _SaveNode(IssueCategory entity) {
            var nodeList = new List<IssueCategory>();
            var node = entity;
            while (node != null) {
                nodeList.Add(node);
                node = node.Parent;
            }
            entity.Depth = nodeList.Count;
            _context.Update(entity);

            nodeList.Reverse();

            entity.IssueCategoryName1 = "";
            entity.IssueCategoryName2 = "";
            entity.IssueCategoryName3 = "";
            entity.IssueCategoryName4 = "";
            if (nodeList.Count > 0) {
                entity.IssueCategory1 = nodeList[0];
                entity.IssueCategoryName1 = nodeList[0].Name;
            }
            if (nodeList.Count > 1) {
                entity.IssueCategory2 = nodeList[1];
                entity.IssueCategoryName2 = nodeList[1].Name;
            }
            if (nodeList.Count > 2) {
                entity.IssueCategory3 = nodeList[2];
                entity.IssueCategoryName3 = nodeList[2].Name;
            }
            if (nodeList.Count > 3) {
                entity.IssueCategory4 = nodeList[3];
                entity.IssueCategoryName4 = nodeList[3].Name;
            }
            entity.Path = string.Join(" > ", nodeList.Select(x => x.Name).ToArray());
            _context.Update(entity);

            if (entity.Childs != null) {
                foreach (var child in entity.Childs) {
                    _SaveNode(child);
                }
            }
            _context.SaveChanges();
        }

        // 노드 이동
        private void _MoveNode(IssueCategory entity, int position) {
            var sibling = (from s in _context.IssueCategory
                           where s.Parent == entity.Parent
                           select s);
            var oldpos = entity.Position;

            foreach (var item in (from s in sibling
                                  where s.Position >= position
                                  orderby s.Position
                                  ascending
                                  select s)) {
                item.Position++;
                _context.Update(item);
            }
            entity.Position = position;
            _context.Update(entity);
            foreach (var item in (from s in sibling
                                  where s.Position > oldpos
                                  orderby s.Position
                                  ascending
                                  select s)) {
                item.Position--;
                _context.Update(item);
            }
            _context.SaveChanges();
        }

        private void _ArrangePosition(IssueCategory node, int pos, int depth) {
            if (node != null) {
                node.Position = pos;
                node.Depth = depth;
                if (_GetChilds(node).Count() == 0) {
                    node.IsLeafNode = true;
                }
                _context.Update(node);
            }
            IQueryable<IssueCategory> query = (from s in _context.IssueCategory
                                               where s.Parent == node && s.IsDelete == false
                                               orderby s.Position ascending
                                               select s);

            int childIndex = 0;
            foreach (var item in query) {
                _ArrangePosition(item, childIndex + 1, depth + 1);
                childIndex++;
            }
            _context.SaveChanges();
        }
    }

}
